using System;
using System.Collections;
using System.Collections.Generic;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="List{T}"/>.
/// </summary>
/// <typeparam name="T">The type contained in the <see cref="List{T}"/>.</typeparam>
public sealed class NetworkedList<T> : INetworkable, IEnumerable<T> where T : INetworkable
{
	/// <summary>
	/// The underlying <see cref="List{T}"/> being contained inside.
	/// </summary>
	public List<T> Value
	{
		get => _value;
		set
		{
			_value = value;

			_changes.Clear();
			_changes.Add( (ListChangeType.Clear, default) );
			foreach ( var val in value )
				_changes.Add( (ListChangeType.Add, val) );
		}
	}
	/// <summary>
	/// See <see cref="Value"/>.
	/// </summary>
	private List<T> _value = null!;

	/// <summary>
	/// Gets the total number of elements the internal data structure can hold without resizing.
	/// <returns>The number of elements that the <see cref="List{T}"/> can contain before resizing is required.</returns>
	/// </summary>
	public int Capacity => Value.Capacity;
	/// <summary>
	/// Gets the number of elements contained in the <see cref="List{T}"/>.
	/// <returns>The number of elements contained in the <see cref="List{T}"/>.</returns>
	/// </summary>
	public int Count => Value.Count;

	/// <summary>
	/// A list containing all the changes made to the list since the last networking update.
	/// </summary>
	private readonly List<(ListChangeType, T?)> _changes = new();

	/// <summary>
	/// Initializes a default instance of <see cref="NetworkedList{T}"/>.
	/// </summary>
	public NetworkedList()
	{
		Value = new List<T>();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedList{T}"/> with a pre-allocated <see cref="HashSet{T}"/>.
	/// </summary>
	/// <param name="list">The pre-allocated <see cref="HashSet{T}"/> to start with.</param>
	public NetworkedList( List<T> list )
	{
		Value = list;
	}

	/// <summary>
	/// Adds an object to the end of the <see cref="List{T}"/>.
	/// </summary>
	/// <param name="item">The object to be added to the end of the <see cref="List{T}"/>. The value can be null for reference types.</param>
	public void Add( T item )
	{
		Value.Add( item );
		_changes.Add( (ListChangeType.Add, item) );
	}

	/// <summary>
	/// Determines whether an element is in the <see cref="List{T}"/>.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="List{T}"/>. The value can be null for reference types.</param>
	/// <returns>true if item is found in the <see cref="List{T}"/>; otherwise, false.</returns>
	public bool Contains( T item )
	{
		return Value.Contains( item );
	}

	/// <summary>
	/// Removes the first occurrence of a specific object from the <see cref="List{T}"/>.
	/// </summary>
	/// <param name="item">The object to remove from the <see cref="List{T}"/>. The value can be null for reference types.</param>
	public void Remove( T item )
	{
		Value.Remove( item );
		_changes.Add( (ListChangeType.Remove, item) );
	}

	/// <summary>
	/// Removes all elements from the <see cref="List{T}"/>.
	/// </summary>
	public void Clear()
	{
		Value.Clear();
		_changes.Clear();
		_changes.Add( (ListChangeType.Clear, default) );
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return Value.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedList{T}"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedList{T}"/> has changed.</returns>
	public bool Changed()
	{
		return _changes.Count > 0;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedList{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		Value = new List<T>();
		var listLength = reader.ReadInt32();
		for ( var i = 0; i < listLength; i++ )
			Value.Add( reader.ReadNetworkable<T>() );
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedList{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when a enumerated <see cref="ListChangeType"/> is invalid.</exception>
	public void DeserializeChanges( NetworkReader reader )
	{
		var changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
		{
			var action = (ListChangeType)reader.ReadByte();
			T? value = default;
			if ( reader.ReadBoolean() )
				value = reader.ReadNetworkable<T>();

			switch ( action )
			{
				case ListChangeType.Add:
					Add( value! );
					break;
				case ListChangeType.Remove:
					Remove( value! );
					break;
				case ListChangeType.Clear:
					Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( action ) );
			}
		}
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedList{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( Value.Count );
		foreach ( var item in Value )
			writer.WriteNetworkable( item );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedList{T}"/>.
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
	/// Returns the underlying list contained in the <see cref="NetworkedList{T}"/>.
	/// </summary>
	/// <param name="networkedList">The <see cref="NetworkedList{T}"/> to get the list from.</param>
	/// <returns>The underlying list contained in the <see cref="NetworkedList{T}"/>.</returns>
	public static implicit operator List<T>( NetworkedList<T> networkedList )
	{
		return networkedList.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedList{T}"/> that contains the provided <see cref="List{T}"/>.
	/// </summary>
	/// <param name="list">The list to contain in the <see cref="NetworkedList{T}"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedList{T}"/> that contains the provided <see cref="List{T}"/>.</returns>
	public static implicit operator NetworkedList<T>( List<T> list )
	{
		return new NetworkedList<T>( list );
	}

	/// <summary>
	/// Represents a type of change the <see cref="NetworkedList{T}"/> has gone through.
	/// </summary>
	private enum ListChangeType : byte
	{
		Add,
		Remove,
		Clear
	}
}
