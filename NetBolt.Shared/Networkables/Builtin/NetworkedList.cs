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
	/// <inheritdoc/>
	public int NetworkId => 0;
	/// <inheritdoc/>
	public bool SupportEquals => false;
	/// <inheritdoc/>
	public bool SupportLerp => false;

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
	/// The indexer to the underlying <see cref="List{T}"/>.
	/// </summary>
	/// <param name="index">The index to look at.</param>
	/// <returns>The value at that index.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when indexing at a location that is not inside of this <see cref="NetworkedList{T}"/>.</exception>
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
	/// The number of elements that the <see cref="List{T}"/> can contain before resizing is required.
	/// </summary>
	public int Capacity => Value.Capacity;
	/// <summary>
	/// The number of elements contained in the <see cref="List{T}"/>.
	/// </summary>
	public int Count => Value.Count;

	/// <summary>
	/// A list containing all the changes made to the list since the last networking update.
	/// </summary>
	private readonly List<(ListChangeType, T?)> _changes = new();

	/// <summary>
	/// A hash set containing all the indices that have changed since the last networking update.
	/// </summary>
	private readonly HashSet<int> _indicesChanged = new();

	/// <summary>
	/// Whether or not this <see cref="NetworkedList{T}"/> is containing a type that is a <see cref="ComplexNetworkable"/>.
	/// </summary>
	private readonly bool _containingComplexNetworkable = typeof( T ).IsAssignableTo( typeof( ComplexNetworkable ) );

	/// <summary>
	/// Initializes a default instance of <see cref="NetworkedList{T}"/>.
	/// </summary>
	public NetworkedList()
	{
		_value = new List<T>();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedList{T}"/> with a pre-allocated <see cref="HashSet{T}"/>.
	/// </summary>
	/// <param name="list">The pre-allocated <see cref="HashSet{T}"/> to start with.</param>
	public NetworkedList( List<T> list )
	{
		_value = list;
		foreach ( var element in list )
			_changes.Add( (ListChangeType.Add, element) );
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
	/// Returns whether or not the <see cref="NetworkedList{T}"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedList{T}"/> has changed.</returns>
	public bool Changed()
	{
		if ( _changes.Count > 0 || _indicesChanged.Count > 0 )
			return true;

		foreach ( var item in Value )
		{
			if ( INetworkable.HasChanged( typeof( T ), item, item, !_containingComplexNetworkable ) )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedList{T}"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="NetworkedList{T}"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue ) => false;

	/// <summary>
	/// Lerps a <see cref="NetworkedList{T}"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="NetworkedList{T}"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedList{T}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		Value = new List<T>();
		var listLength = reader.ReadInt32();
		if ( _containingComplexNetworkable )
		{
			for ( var i = 0; i < listLength; i++ )
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
			for ( var i = 0; i < listLength; i++ )
				Add( reader.ReadNetworkable<T>() );
		}
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
			var changeType = (ListChangeType)reader.ReadByte();
			switch ( changeType )
			{
				case ListChangeType.Add:
				case ListChangeType.Remove:
					T? value = default;
					if ( reader.ReadBoolean() )
					{
						if ( _containingComplexNetworkable )
						{
							var networkId = reader.ReadInt32();
							var complexNetworkable = ComplexNetworkable.GetById( networkId );
							if ( complexNetworkable is null )
							{
								ILogger.Instance.Error( "Deserialized an unknown {0} (ID: {1})", typeof( T ).Name, networkId );
								continue;
							}

							value = (T)(INetworkable)complexNetworkable;
						}
						else
							value = reader.ReadNetworkable<T>();
					}

					if ( changeType == ListChangeType.Add )
						Add( value! );
					else
						Remove( value! );
					break;
				case ListChangeType.Clear:
					Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( changeType ) );
			}
		}

		changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
		{
			Value[reader.ReadInt32()] = _containingComplexNetworkable
				? (T)(INetworkable)ComplexNetworkable.GetById( reader.ReadInt32() )!
				: reader.ReadNetworkable<T>();
		}

		changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
			Value[reader.ReadInt32()].DeserializeChanges( reader );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedList{T}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( Value.Count );
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
	/// Serializes all changes relating to the <see cref="NetworkedList{T}"/>.
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
				case ListChangeType.Add:
				case ListChangeType.Remove:
					var hasValue = value is not null;
					writer.Write( hasValue );

					if ( hasValue )
					{
						if ( _containingComplexNetworkable )
							writer.Write( value!.NetworkId );
						else
							writer.Write( value! );
					}
					break;
				case ListChangeType.Clear:
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( changeType ) );
			}
		}
		_changes.Clear();

		writer.Write( _indicesChanged.Count );
		foreach ( var itemIndex in _indicesChanged )
		{
			writer.Write( itemIndex );
			if ( _containingComplexNetworkable )
				writer.Write( Value[itemIndex].NetworkId );
			else
				writer.Write( Value[itemIndex] );
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
		for ( var i = 0; i < Value.Count; i++ )
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
	/// Returns the underlying <see cref="List{T}"/> contained in the <see cref="NetworkedList{T}"/>.
	/// </summary>
	/// <param name="networkedList">The <see cref="NetworkedList{T}"/> to get the <see cref="List{T}"/> from.</param>
	/// <returns>The underlying <see cref="List{T}"/> contained in the <see cref="NetworkedList{T}"/>.</returns>
	public static implicit operator List<T>( NetworkedList<T> networkedList )
	{
		return networkedList.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedList{T}"/> that contains the provided <see cref="List{T}"/>.
	/// </summary>
	/// <param name="list">The <see cref="List{T}"/> to contain in the <see cref="NetworkedList{T}"/>.</param>
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
