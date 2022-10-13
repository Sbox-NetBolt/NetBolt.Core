using System.Collections.Generic;
using System.Linq;
using NetBolt.Server;
using NetBolt.Shared.Utility;
#if CLIENT
using Sandbox;
#endif

namespace NetBolt.Shared.Networkables;

/// <summary>
/// Base class for a networkable that contains other <see cref="INetworkable"/>s.
/// </summary>
public abstract partial class BaseNetworkable : INetworkable
{
	/// <summary>
	/// The unique identifier of the networkable.
	/// </summary>
	public int NetworkId { get; }

	/// <summary>
	/// </summary>
	/// <summary>
	/// Deletes the <see cref="BaseNetworkable"/>. You should not be using this after it is invoked.
	/// </summary>
	internal virtual void Delete()
	{
		OnDeleted();

		AllNetworkables.Remove( this );
#if SERVER
		NetBoltGame.Current.OnBaseNetworkableDeleted( this );
#endif
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
			// TODO: handle null values.
			if ( propertyInfo.GetValue( this ) is not INetworkable networkable )
				return false;

			if ( networkable.Changed() )
				return true;
		}

		return false;
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
#if CLIENT
				else
					ClPendingNetworkables.Add( networkId, propertyName );
#endif
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
#if CLIENT
				else
					ClPendingNetworkables.Add( networkId, propertyName );
#endif
			}
			else
			{
				var currentValue = property.GetValue( this );
				(currentValue as INetworkable)!.DeserializeChanges( reader );
				property.SetValue( this, currentValue );
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
				if ( TypeHelper.IsStruct( propertyInfo.PropertyType ) )
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
	internal static IReadOnlyList<BaseNetworkable> All => AllNetworkables;
	/// <summary>
	/// A list of all <see cref="BaseNetworkable"/>
	/// </summary>
	private static readonly List<BaseNetworkable> AllNetworkables = new();
}
