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
	/// <inheritdoc/>
	public int NetworkId => 0;
	/// <inheritdoc/>
	public bool SupportEquals => false;
	/// <inheritdoc/>
	public bool SupportLerp => false;

	/// <summary>
	/// The underlying <see cref="Array"/> being contained inside.
	/// </summary>
	public T[] Value { get; private set; }

	/// <summary>
	/// The indexer to the underlying <see cref="Array"/>.
	/// </summary>
	/// <param name="index">The index to look at.</param>
	public T this[int index]
	{
		get => Value[index];
		set
		{
			Value[index] = value;
			_indicesChanged.Add( index );
		}
	}

	/// <summary>
	/// Gets the total number of elements in the dimensions of the <see cref="Array"/>.
	/// </summary>
	public int Length => Value.Length;

	/// <summary>
	/// A set of indices that have changed since the last time this was networked.
	/// </summary>
	private readonly HashSet<int> _indicesChanged = new();

	/// <summary>
	/// Whether or not this <see cref="NetworkedList{T}"/> is containing a type that is a <see cref="ComplexNetworkable"/>.
	/// </summary>
	private readonly bool _containingComplexNetworkable = typeof( T ).IsAssignableTo( typeof( ComplexNetworkable ) );

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

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)Value).GetEnumerator();
	}

	/// <inheritdoc/>
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
		if ( _indicesChanged.Count > 0 )
			return true;

		foreach ( var item in Value )
		{
			if ( INetworkable.HasChanged( typeof( T ), item, item, !_containingComplexNetworkable ) )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedArray{T}"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="NetworkedArray{T}"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue ) => false;

	/// <summary>
	/// Lerps a <see cref="NetworkedArray{T}"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="NetworkedArray{T}"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		var arrayLength = reader.ReadInt32();
		Value = new T[arrayLength];
		if ( _containingComplexNetworkable )
		{
			for ( var i = 0; i < arrayLength; i++ )
			{
				var networkId = reader.ReadInt32();
				var complexNetworkable = ComplexNetworkable.GetOrRequestById( networkId, complexNetworkable => Value[i] = (T)(INetworkable)complexNetworkable );

				if ( complexNetworkable is not null )
					Value[i] = (T)(INetworkable)complexNetworkable;
			}
		}
		else
		{
			for ( var i = 0; i < arrayLength; i++ )
				Value[i] = reader.ReadNetworkable<T>();
		}
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void DeserializeChanges( NetworkReader reader )
	{
		var changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
		{
			var index = reader.ReadInt32();
			Value[index] = _containingComplexNetworkable
				? (T)(INetworkable)ComplexNetworkable.GetOrRequestById( reader.ReadInt32(), complexNetworkable => Value[index] = (T)(INetworkable)complexNetworkable )!
				: reader.ReadNetworkable<T>();
		}

		changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
			Value[reader.ReadInt32()].DeserializeChanges( reader );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( Value.Length );
		if ( _containingComplexNetworkable )
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
	/// Serializes all changes relating to the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		writer.Write( _indicesChanged.Count );
		foreach ( var index in _indicesChanged )
		{
			writer.Write( index );
			writer.Write( Value[index] );
		}
		_indicesChanged.Clear();

		if ( _containingComplexNetworkable )
		{
			writer.Write( 0 );
			return;
		}

		var networkableCountPos = writer.BaseStream.Position;
		writer.BaseStream.Position += sizeof( int );
		var networkableChangeCount = 0;
		for ( var i = 0; i < Value.Length; i++ )
		{
			var item = Value[i];
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
	/// Returns the underlying <see cref="Array"/> contained in the <see cref="NetworkedArray{T}"/>.
	/// </summary>
	/// <param name="networkedArray">The <see cref="NetworkedArray{T}"/> to get the <see cref="Array"/> from.</param>
	/// <returns>The underlying <see cref="Array"/> contained in the <see cref="NetworkedArray{T}"/>.</returns>
	public static implicit operator Array( NetworkedArray<T> networkedArray )
	{
		return networkedArray.Value;
	}
}
