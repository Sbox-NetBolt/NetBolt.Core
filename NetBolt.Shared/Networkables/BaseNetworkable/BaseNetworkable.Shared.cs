using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables;

/// <summary>
/// Base class for a networkable that contains other <see cref="INetworkable"/>s.
/// </summary>
public abstract partial class BaseNetworkable : INetworkable
{
	/// <summary>
	/// The unique identifier of the networkable.
	/// </summary>
	public int NetworkId { get; internal set; }

	/// <summary>
	/// A dictionary to contain the last networked references.
	/// </summary>
	private readonly Dictionary<string, INetworkable?> _referenceBucket = new();

	/// <summary>
	/// A <see cref="PropertyInfo"/> cache of all networked properties.
	/// </summary>
	internal readonly Dictionary<string, IProperty> PropertyNameCache = new();

	/// <summary>
	/// Initializes a new instance of <see cref="BaseNetworkable"/>.
	/// </summary>
	protected BaseNetworkable()
	{
		if ( Realm.IsServer )
			NetworkId = StepNextId();

		foreach ( var property in IGlue.Instance.TypeLibrary.GetAllProperties( GetType() )
					 .Where( property => property.PropertyType.IsAssignableTo( typeof( INetworkable ) ) ) )
		{
			if ( property.HasAttribute<NoNetworkAttribute>() )
				continue;

			if ( Realm.IsClient && property.HasAttribute<LerpAttribute>() )
				LerpBucket.Add( property.Name, (null, null) );

			if ( IGlue.Instance.TypeLibrary.IsStruct( property.PropertyType ) )
				_referenceBucket.Add( property.Name, null );

			PropertyNameCache.Add( property.Name, property );
		}

		AllNetworkables.Add( this );
		if ( Realm.IsServer )
			IGlue.Instance.Server.OnBaseNetworkableCreated( this );
	}

	/// <summary>
	/// Deletes the <see cref="BaseNetworkable"/>. You should not be using this after it is invoked.
	/// </summary>
	public virtual void Delete()
	{
		OnDeleted();

		AllNetworkables.Remove( this );
		if ( Realm.IsServer )
			IGlue.Instance.Server.OnBaseNetworkableDeleted( this );
	}

	/// <summary>
	/// Invoked when the <see cref="BaseNetworkable"/> is being deleted.
	/// </summary>
	protected virtual void OnDeleted()
	{
	}

	/// <summary>
	/// Returns whether or not the <see cref="BaseNetworkable"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="BaseNetworkable"/> has changed.</returns>
	public bool Changed()
	{
		foreach ( var propertyInfo in PropertyNameCache.Values )
		{
			if ( propertyInfo.PropertyType.IsAssignableTo( typeof( BaseNetworkable ) ) )
				continue;

			// TODO: handle null values.
			if ( propertyInfo.GetValue( this ) is not INetworkable networkable )
				return false;

			if ( networkable.Changed() )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Lerps a <see cref="BaseNetworkable"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="BaseNetworkable"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="BaseNetworkable"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public virtual void Deserialize( NetworkReader reader )
	{
		var count = reader.ReadInt32();
		for ( var i = 0; i < count; i++ )
		{
			var propertyName = reader.ReadString();
			var propertyInfo = PropertyNameCache[propertyName];
			if ( propertyInfo.PropertyType.IsAssignableTo( typeof( BaseNetworkable ) ) )
			{
				var networkId = reader.ReadInt32();
				var networkable = All.FirstOrDefault( networkable => networkable.NetworkId == networkId );
				if ( networkable is not null )
					propertyInfo.SetValue( this, networkable );
				else if ( Realm.IsClient )
					PendingNetworkables.Add( networkId, propertyName );
			}
			else
				propertyInfo.SetValue( this, reader.ReadNetworkable() );
		}
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="BaseNetworkable"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public virtual void DeserializeChanges( NetworkReader reader )
	{
		var changedCount = reader.ReadInt32();
		for ( var i = 0; i < changedCount; i++ )
		{
			var propertyName = reader.ReadString();
			var property = PropertyNameCache[propertyName];
			if ( property.PropertyType.IsAssignableTo( typeof( BaseNetworkable ) ) )
			{
				var networkId = reader.ReadInt32();
				var networkable = All.FirstOrDefault( networkable => networkable.NetworkId == networkId );
				if ( networkable is not null )
					property.SetValue( this, networkable );
				else if ( Realm.IsClient )
					PendingNetworkables.Add( networkId, propertyName );
			}
			else
			{
				if ( Realm.IsClient && property.HasAttribute<LerpAttribute>() )
				{
					var oldValue = property.GetValue( this ) as INetworkable;
					var newValue = property.GetValue( this ) as INetworkable;
					newValue!.DeserializeChanges( reader );

					LerpBucket[propertyName] = (oldValue, newValue);
				}
				else
				{
					var currentValue = property.GetValue( this );
					(currentValue as INetworkable)!.DeserializeChanges( reader );
					if ( IGlue.Instance.TypeLibrary.IsStruct( property.PropertyType ) )
						property.SetValue( this, currentValue );
				}
			}
		}
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="BaseNetworkable"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public virtual void Serialize( NetworkWriter writer )
	{
		writer.Write( PropertyNameCache.Count );
		foreach ( var (propertyName, propertyInfo) in PropertyNameCache )
		{
			writer.Write( propertyName );
			var networkable = (INetworkable)propertyInfo.GetValue( this )!;
			if ( propertyInfo.PropertyType.IsAssignableTo( typeof( BaseNetworkable ) ) &&
				 networkable is BaseNetworkable baseNetworkable )
				writer.Write( baseNetworkable.NetworkId );
			else
				writer.WriteNetworkable( networkable );
		}
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="BaseNetworkable"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public virtual void SerializeChanges( NetworkWriter writer )
	{
		var numChanged = 0;
		var changedStreamPos = writer.BaseStream.Position;
		writer.BaseStream.Position += sizeof( int );

		foreach ( var (propertyName, propertyInfo) in PropertyNameCache )
		{
			var networkable = (INetworkable)propertyInfo.GetValue( this )!;
			if ( !networkable.Changed() )
				continue;

			numChanged++;
			writer.Write( propertyName );
			if ( networkable is BaseNetworkable baseNetworkable )
				writer.Write( baseNetworkable.NetworkId );
			else
			{
				writer.WriteNetworkableChanges( ref networkable );
				if ( IGlue.Instance.TypeLibrary.IsStruct( propertyInfo.PropertyType ) )
					propertyInfo.SetValue( this, networkable );
			}
		}

		var tempPos = writer.BaseStream.Position;
		writer.BaseStream.Position = changedStreamPos;
		writer.Write( numChanged );
		writer.BaseStream.Position = tempPos;
	}

	/// <summary>
	/// Returns a string that represents the <see cref="BaseNetworkable"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="BaseNetworkable"/>.</returns>
	public override string ToString()
	{
		return $"BaseNetworkable (ID: {NetworkId})";
	}

	/// <summary>
	/// A read-only list of all <see cref="BaseNetworkable"/>s.
	/// </summary>
	public static IReadOnlyList<BaseNetworkable> All => AllNetworkables;
	/// <summary>
	/// A list of all <see cref="BaseNetworkable"/>
	/// </summary>
	private static readonly List<BaseNetworkable> AllNetworkables = new();
}
