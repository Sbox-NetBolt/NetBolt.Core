using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables;

/// <summary>
/// Base class for a networkable that contains other <see cref="INetworkable"/>s.
/// </summary>
public abstract partial class ComplexNetworkable : INetworkable
{
	/// <inheritdoc/>
	public int NetworkId { get; internal set; }
	/// <inheritdoc/>
	public bool SupportEquals => true;
	/// <inheritdoc/>
	public bool SupportLerp => false;

	/// <summary>
	/// A dictionary to contain the last networked references.
	/// </summary>
	internal readonly Dictionary<string, INetworkable?> _referenceBucket = new();

	/// <summary>
	/// A <see cref="PropertyInfo"/> cache of all networked properties.
	/// </summary>
	internal readonly Dictionary<string, IProperty> PropertyNameCache = new();

	/// <summary>
	/// Initializes a default instance of <see cref="ComplexNetworkable"/>.
	/// </summary>
	protected ComplexNetworkable()
	{
		if ( Realm.IsServer )
			NetworkId = StepNextId();

		foreach ( var property in ITypeLibrary.Instance.GetAllProperties( GetType() ) )
		{
			if ( !property.IsNetworkable )
				continue;

			if ( Realm.IsClient && property.HasAttribute<LerpAttribute>() )
				LerpBucket.Add( property.Name, (null, null) );

			_referenceBucket.Add( property.Name, (INetworkable?)property.GetValue( this ) );
			PropertyNameCache.Add( property.Name, property );
		}

		AllNetworkables.Add( this );
		if ( Realm.IsServer )
			INetBoltServer.Instance.OnComplexNetworkableCreated( this );
	}

	/// <summary>
	/// Deletes the <see cref="ComplexNetworkable"/>. You should not be using this after it is invoked.
	/// </summary>
	public virtual void Delete()
	{
		OnDeleted();

		AllNetworkables.Remove( this );
		if ( Realm.IsServer )
			INetBoltServer.Instance.OnComplexNetworkableDeleted( this );
	}

	/// <summary>
	/// Invoked when the <see cref="ComplexNetworkable"/> is being deleted.
	/// </summary>
	protected virtual void OnDeleted()
	{
	}

	/// <summary>
	/// Returns whether or not the <see cref="ComplexNetworkable"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="ComplexNetworkable"/> has changed.</returns>
	public bool Changed()
	{
		foreach ( var (propertyName, property) in PropertyNameCache )
		{
			if ( INetworkable.HasChanged( property.PropertyType, _referenceBucket[propertyName], (INetworkable)property.GetValue( this )!,
				!property.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) ) )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns whether or not the <see cref="ComplexNetworkable"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="ComplexNetworkable"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue )
	{
		return NetworkId == (oldValue?.NetworkId ?? -1);
	}

	/// <summary>
	/// Lerps a <see cref="ComplexNetworkable"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="ComplexNetworkable"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="ComplexNetworkable"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public virtual void Deserialize( NetworkReader reader )
	{
		var count = reader.ReadInt32();
		for ( var i = 0; i < count; i++ )
		{
			var propertyName = reader.ReadString();
			var property = PropertyNameCache[propertyName];
			if ( property.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) )
			{
				var networkId = reader.ReadInt32();
				INetworkable? networkable;

				if ( Realm.IsClient )
				{
					networkable = GetOrRequestById( networkId, complexNetworkable =>
					{
						property.SetValue( this, complexNetworkable );
						_referenceBucket[propertyName] = complexNetworkable;
					} );
				}
				else
					networkable = GetById( networkId );

				property.SetValue( this, networkable );
				_referenceBucket[propertyName] = networkable;
			}
			else
			{
				INetworkable? networkable;
				if ( reader.ReadBoolean() )
					networkable = reader.ReadNetworkable();
				else
					networkable = null;

				property.SetValue( this, networkable );
				_referenceBucket[propertyName] = networkable;
			}
		}
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="ComplexNetworkable"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public virtual void DeserializeChanges( NetworkReader reader )
	{
		var changedCount = reader.ReadInt32();
		for ( var i = 0; i < changedCount; i++ )
		{
			var propertyName = reader.ReadString();
			var property = PropertyNameCache[propertyName];
			if ( property.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) )
			{
				var networkId = reader.ReadInt32();
				INetworkable? networkable;

				if ( Realm.IsClient )
				{
					networkable = GetOrRequestById( networkId, complexNetworkable =>
					{
						property.SetValue( this, complexNetworkable );
						_referenceBucket[propertyName] = complexNetworkable;
					} );
				}
				else
					networkable = GetById( networkId );

				property.SetValue( this, networkable );
				_referenceBucket[propertyName] = networkable;
			}
			else if ( Realm.IsClient && property.HasAttribute<LerpAttribute>() )
			{
				var oldValue = (INetworkable)property.GetValue( this )!;
				var newValue = (INetworkable)property.GetValue( this )!;
				newValue.DeserializeChanges( reader );

				LerpBucket[propertyName] = (oldValue, newValue);
				_referenceBucket[propertyName] = newValue;
			}
			else
			{
				if ( property.GetValue( this ) is not INetworkable currentValue )
				{
					ILogger.Instance.Error( "{0} is missing its value on {1}", this, propertyName );
					continue;
				}

				currentValue.DeserializeChanges( reader );
				if ( ITypeLibrary.Instance.IsStruct( property.PropertyType ) )
					property.SetValue( this, currentValue );

				_referenceBucket[propertyName] = currentValue;
			}
		}
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ComplexNetworkable"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public virtual void Serialize( NetworkWriter writer )
	{
		writer.Write( PropertyNameCache.Count );
		foreach ( var (propertyName, propertyInfo) in PropertyNameCache )
		{
			writer.Write( propertyName );
			var networkable = propertyInfo.GetValue( this ) as INetworkable;
			if ( propertyInfo.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) )
				writer.Write( networkable?.NetworkId ?? -1 );
			else
			{
				var hasValue = networkable is not null;
				writer.Write( hasValue );
				if ( hasValue )
					writer.Write( networkable! );
			}
		}
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="ComplexNetworkable"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public virtual void SerializeChanges( NetworkWriter writer )
	{
		var numChanged = 0;
		var changedStreamPos = writer.BaseStream.Position;
		writer.BaseStream.Position += sizeof( int );

		foreach ( var (propertyName, property) in PropertyNameCache )
		{
			var networkable = (INetworkable)property.GetValue( this )!;
			if ( !INetworkable.HasChanged( property.PropertyType, _referenceBucket[propertyName], networkable,
				!property.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) ) )
				continue;

			numChanged++;
			writer.Write( propertyName );
			if ( property.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) )
				writer.Write( networkable?.NetworkId ?? -1 );
			else
			{
				writer.WriteChanges( ref networkable );
				if ( ITypeLibrary.Instance.IsStruct( property.PropertyType ) )
					property.SetValue( this, networkable );
			}

			_referenceBucket[propertyName] = networkable;
		}

		var tempPos = writer.BaseStream.Position;
		writer.BaseStream.Position = changedStreamPos;
		writer.Write( numChanged );
		writer.BaseStream.Position = tempPos;
	}

	/// <summary>
	/// Returns a string that represents the <see cref="ComplexNetworkable"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="ComplexNetworkable"/>.</returns>
	public override string ToString()
	{
		return $"{nameof( ComplexNetworkable )} (ID: {NetworkId})";
	}

	/// <summary>
	/// A read-only list of all <see cref="ComplexNetworkable"/>s.
	/// </summary>
	public static IReadOnlyList<ComplexNetworkable> All => AllNetworkables;
	/// <summary>
	/// A list of all <see cref="ComplexNetworkable"/>
	/// </summary>
	private static readonly List<ComplexNetworkable> AllNetworkables = new();

	/// <summary>
	/// Gets a <see cref="ComplexNetworkable"/> by its unique network identifier.
	/// </summary>
	/// <param name="networkId">The unique network identifier of the <see cref="ComplexNetworkable"/>.</param>
	/// <returns>The <see cref="ComplexNetworkable"/> that was found. Null if <see ref="networkId"/> is -1.</returns>
	public static ComplexNetworkable? GetById( int networkId )
	{
		return networkId == -1 ? null : All.First( complexNetworkable => complexNetworkable.NetworkId == networkId );
	}
}
