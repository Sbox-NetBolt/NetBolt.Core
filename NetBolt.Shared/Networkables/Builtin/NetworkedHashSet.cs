using System;
using System.Collections;
using System.Collections.Generic;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="HashSet{T}"/>.
/// </summary>
/// <typeparam name="T">The type contained in the <see cref="HashSet{T}"/>.</typeparam>
public sealed class NetworkedHashSet<T> : INetworkable, IEnumerable<T> where T : INetworkable
{
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

	/// <summary>
	/// The list of changes that have happened since the last time this was networked.
	/// </summary>
	private readonly List<(HashSetChangeType, T?)> _changes = new();

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
		return _changes.Count > 0;
	}

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
		var listLength = reader.ReadInt32();
		for ( var i = 0; i < listLength; i++ )
			Value.Add( reader.ReadNetworkable<T>() );
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
			T? value = default;
			if ( reader.ReadBoolean() )
				value = reader.ReadNetworkable<T>();

			switch ( action )
			{
				case HashSetChangeType.Add:
					Add( value! );
					break;
				case HashSetChangeType.Remove:
					Remove( value! );
					break;
				case HashSetChangeType.Clear:
					Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( action ) );
			}
		}
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( Value.Count );
		foreach ( var item in Value )
			writer.WriteNetworkable( item );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedHashSet{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		writer.Write( _changes.Count );
		foreach ( var change in _changes )
		{
			writer.Write( (byte)change.Item1 );
			var isNull = change.Item2 is null;
			writer.Write( isNull );

			if ( !isNull )
				writer.WriteNetworkable( change.Item2! );
		}
		_changes.Clear();
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
