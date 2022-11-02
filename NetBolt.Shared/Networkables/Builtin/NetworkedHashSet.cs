using System;
using System.Collections;
using System.Collections.Generic;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="HashSet{T}"/>.
/// </summary>
/// <typeparam name="T">The type contained in the <see cref="HashSet{T}"/>.</typeparam>
public sealed class NetworkedHashSet<T> : INetworkable,
	ICollection<T>, IReadOnlyCollection<T>,
	IEnumerable<T>, IEnumerable
	where T : INetworkable
{
	/// <inheritdoc/>
	public int NetworkId => 0;
	/// <inheritdoc/>
	public bool SupportEquals => false;
	/// <inheritdoc/>
	public bool SupportLerp => false;

	/// <summary>
	/// The underlying <see cref="HashSet{T}"/> being contained inside.
	/// </summary>
	public HashSet<T> Value
	{
		get => _value;
		set
		{
			_value = value;

			_changes.Clear();
			_changes.Add( (HashSetChangeType.Clear, default) );
			foreach ( var val in value )
				_changes.Add( (HashSetChangeType.Add, val) );
		}
	}
	/// <summary>
	/// See <see cref="Value"/>.
	/// </summary>
	private HashSet<T> _value = null!;

	/// <summary>
	/// The number of elements that are contained in the set.
	/// </summary>
	public int Count => Value.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <summary>
	/// A list of changes that have happened since the last time this was networked.
	/// </summary>
	private readonly List<(HashSetChangeType, T?)> _changes = new();

	/// <summary>
	/// Whether or not this <see cref="NetworkedHashSet{T}"/> is containing a type that is a <see cref="ComplexNetworkable"/>.
	/// </summary>
	private readonly bool _containingBaseNetworkable = typeof( T ).IsAssignableTo( typeof( ComplexNetworkable ) );

	/// <summary>
	/// Whether or not this <see cref="NetworkedHashSet{T}"/> is containing a type that is a struct.
	/// </summary>
	private readonly bool _containingStruct = ITypeLibrary.Instance.IsStruct( typeof( T ) );

	/// <summary>
	/// Initializes a default instance of <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	public NetworkedHashSet()
	{
		_value = new HashSet<T>();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedHashSet{T}"/> with a pre-allocated <see cref="HashSet{T}"/>.
	/// </summary>
	/// <param name="hashSet">The pre-allocated <see cref="HashSet{T}"/> to start with.</param>
	public NetworkedHashSet( HashSet<T> hashSet )
	{
		_value = hashSet;
		foreach ( var element in hashSet )
			_changes.Add( (HashSetChangeType.Add, element) );
	}

	/// <summary>
	/// Adds the specified element to a set.
	/// </summary>
	/// <param name="item">The element to add to the set.</param>
	/// <returns>true if the element is added to the <see cref="HashSet{T}"/> object; false if the element is already present.</returns>
	public bool Add( T item )
	{
		var result = Value.Add( item );
		if ( !result )
			return false;

		_changes.Add( (HashSetChangeType.Add, item) );
		return true;
	}

	/// <summary>
	/// Determines whether a <see cref="HashSet{T}"/> object contains the specified element.
	/// </summary>
	/// <param name="item">The element to locate in the <see cref="HashSet{T}"/> object.</param>
	/// <returns>true if the <see cref="HashSet{T}"/> object contains the specified element; otherwise, false.</returns>
	public bool Contains( T item )
	{
		return Value.Contains( item );
	}

	/// <summary>
	/// Removes the specified element from a <see cref="HashSet{T}"/> object.
	/// </summary>
	/// <param name="item">The element to remove.</param>
	/// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if item is not found in the <see cref="HashSet{T}"/> object.</returns>
	public bool Remove( T item )
	{
		var result = Value.Remove( item );
		if ( !result )
			return false;

		_changes.Add( (HashSetChangeType.Remove, item) );
		return true;
	}

	/// <summary>
	/// Removes all elements from a <see cref="HashSet{T}"/> object.
	/// </summary>
	public void Clear()
	{
		Value.Clear();
		_changes.Clear();
		_changes.Add( (HashSetChangeType.Clear, default) );
	}

	/// <inheritdoc/>
	void ICollection<T>.Add( T item )
	{
		Add( item );
	}

	/// <inheritdoc/>
	public void CopyTo( T[] array, int arrayIndex )
	{
		Value.CopyTo( array, arrayIndex );
	}

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		return Value.GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedHashSet{T}"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedHashSet{T}"/> has changed.</returns>
	public bool Changed()
	{
		if ( _changes.Count > 0 )
			return true;

		if ( _containingStruct )
			return false;

		foreach ( var item in Value )
		{
			if ( INetworkable.HasChanged( typeof( T ), item, item, !_containingBaseNetworkable ) )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedHashSet{T}"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="NetworkedHashSet{T}"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue ) => false;

	/// <summary>
	/// Lerps a <see cref="NetworkedHashSet{T}"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="NetworkedHashSet{T}"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		Value = new HashSet<T>();
		var hashSetLength = reader.ReadInt32();
		if ( _containingBaseNetworkable )
		{
			for ( var i = 0; i < hashSetLength; i++ )
			{
				var networkId = reader.ReadInt32();
				var complexNetworkable = ComplexNetworkable.GetById( networkId );
				if ( complexNetworkable is null )
				{
					ILogger.Instance.Error( "Deserialized an unknown {0} (ID: {1})", typeof( T ).Name, networkId );
					continue;
				}

				Add( (T)(INetworkable)complexNetworkable );
			}
		}
		else
		{
			for ( var i = 0; i < hashSetLength; i++ )
				Add( reader.ReadNetworkable<T>() );
		}
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when a enumerated <see cref="HashSetChangeType"/> is invalid.</exception>
	public void DeserializeChanges( NetworkReader reader )
	{
		var changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
		{
			var action = (HashSetChangeType)reader.ReadByte();
			switch ( action )
			{
				case HashSetChangeType.Add:
				case HashSetChangeType.Remove:
					T? value = default;
					if ( reader.ReadBoolean() )
					{
						if ( _containingBaseNetworkable )
							value = (T)(INetworkable)ComplexNetworkable.GetById( reader.ReadInt32() )!;
						else
							value = reader.ReadNetworkable<T>();
					}

					if ( action == HashSetChangeType.Add )
						Add( value! );
					else
						Remove( value! );
					break;
				case HashSetChangeType.Clear:
					Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( action ) );
			}
		}

		if ( _containingStruct )
			return;

		changeCount = reader.ReadInt32();
		if ( changeCount == 0 )
			return;

		var nextIndex = reader.ReadInt32();
		var j = -1;
		var numProcessed = 0;
		foreach ( var item in Value )
		{
			j++;
			if ( j != nextIndex )
				continue;

			numProcessed++;
			item.DeserializeChanges( reader );

			if ( numProcessed == changeCount )
				break;

			nextIndex = reader.ReadInt32();
		}
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( Value.Count );
		if ( _containingBaseNetworkable )
		{
			foreach ( var item in Value )
				writer.Write( item.NetworkId );
		}
		else
		{
			foreach ( var item in Value )
				writer.Write( item );
		}
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		writer.Write( _changes.Count );
		foreach ( var (changeType, value) in _changes )
		{
			writer.Write( (byte)changeType );
			switch ( changeType )
			{
				case HashSetChangeType.Add:
				case HashSetChangeType.Remove:
					var hasValue = value is not null;
					writer.Write( hasValue );
					if ( hasValue )
					{
						if ( _containingBaseNetworkable )
							writer.Write( (value as ComplexNetworkable)!.NetworkId );
						else
							writer.Write( value! );
					}
					break;
				case HashSetChangeType.Clear:
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( changeType ) );
			}
		}
		_changes.Clear();

		if ( _containingStruct || _containingBaseNetworkable )
		{
			writer.Write( 0 );
			return;
		}

		var networkableCountPos = writer.BaseStream.Position;
		writer.BaseStream.Position += sizeof( int );
		var networkableChangeCount = 0;
		var i = -1;
		foreach ( var item in Value )
		{
			i++;
			if ( !INetworkable.HasChanged( typeof( T ), item, item, true ) )
				continue;

			networkableChangeCount++;
			writer.Write( i );
			item.SerializeChanges( writer );
		}

		var tempPos = writer.BaseStream.Position;
		writer.BaseStream.Position = networkableCountPos;
		writer.Write( networkableChangeCount );
		writer.BaseStream.Position = tempPos;
	}

	/// <summary>
	/// Returns the underlying <see cref="HashSet{T}"/> contained in the <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	/// <param name="networkedHashSet">The <see cref="NetworkedHashSet{T}"/> to get the <see cref="HashSet{T}"/> from.</param>
	/// <returns>The underlying <see cref="HashSet{T}"/> contained in the <see cref="NetworkedHashSet{T}"/>.</returns>
	public static implicit operator HashSet<T>( NetworkedHashSet<T> networkedHashSet )
	{
		return networkedHashSet.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedHashSet{T}"/> that contains the provided <see cref="HashSet{T}"/>.
	/// </summary>
	/// <param name="hashSet">The <see cref="HashSet{T}"/> to contain in the <see cref="NetworkedHashSet{T}"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedHashSet{T}"/> that contains the provided <see cref="HashSet{T}"/>.</returns>
	public static implicit operator NetworkedHashSet<T>( HashSet<T> hashSet )
	{
		return new NetworkedHashSet<T>( hashSet );
	}

	/// <summary>
	/// Represents a type of change the <see cref="NetworkedHashSet{T}"/> has gone through.
	/// </summary>
	private enum HashSetChangeType : byte
	{
		Add,
		Remove,
		Clear
	}
}
