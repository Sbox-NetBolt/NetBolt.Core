using System;
using System.Collections;
using System.Collections.Generic;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="Array"/>.
/// </summary>
/// <typeparam name="T">The type contained in the <see cref="Array"/>.</typeparam>
public sealed class NetworkedArray<T> : INetworkable, IEnumerable<T> where T : INetworkable
{
	/// <summary>
	/// The underlying <see cref="Array"/> being contained inside.
	/// </summary>
	public T[] Value { get; private set; }
	
	/// <summary>
	/// The indexer to the underlying <see cref="Array"/>.
	/// </summary>
	/// <param name="index">The index to look at.</param>
	public T this[ int index ]
	{
		get => Value[index];
		set
		{
			Value[index] = value;
			_indicesChanged.Add( index );
		}
	}

	/// <summary>
	/// A set of indices that have changed since the last time this was networked.
	/// </summary>
	private readonly HashSet<int> _indicesChanged = new();

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedArray{T}"/> with a pre-allocated <see cref="Array"/>.
	/// </summary>
	/// <param name="array">The pre-allocated <see cref="Array"/> to start with.</param>
	public NetworkedArray( T[] array )
	{
		Value = array;
		for ( var i = 0; i < array.Length; i++ )
			_indicesChanged.Add( i );
	}
	
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedArray{T}"/> with the size of the underlying <see cref="Array"/>.
	/// </summary>
	/// <param name="size">The size of the underlying <see cref="Array"/>.</param>
	public NetworkedArray( int size )
	{
		Value = new T[size];
	}

	/// <summary>
	/// Initializes a default instance of <see cref="NetworkedArray{T}"/>.
	/// </summary>
	public NetworkedArray()
	{
		Value = Array.Empty<T>();
	}
	
	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)Value).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedArray{T}"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedArray{T}"/> has changed.</returns>
	public bool Changed()
	{
		return _indicesChanged.Count > 0;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		var arrayLength = reader.ReadInt32();
		Value = new T[arrayLength];
		for ( var i = 0; i < Value.Length; i++ )
			Value[i] = reader.ReadNetworkable<T>();
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void DeserializeChanges( NetworkReader reader )
	{
		var changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
			Value[reader.ReadInt32()] = reader.ReadNetworkable<T>();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( Value.Length );
		foreach ( var element in Value )
			writer.WriteNetworkable( element );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		writer.Write( _indicesChanged.Count );
		foreach ( var index in _indicesChanged )
		{
			writer.Write( index );
			writer.WriteNetworkable( Value[index] );
		}
		_indicesChanged.Clear();
	}
	
	/// <summary>
	/// Returns the underlying <see cref="Array"/> contained in the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="networkedArray">The <see cref="NetworkedArray{T}"/> to get the <see cref="Array"/> from.</param>
	/// <returns>The underlying <see cref="Array"/> contained in the <see cref="NetworkedArray{T}"/>.</returns>
	public static implicit operator Array( NetworkedArray<T> networkedArray )
	{
		return networkedArray.Value;
	}
}
