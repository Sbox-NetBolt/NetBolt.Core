using NetBolt.Shared.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The key type contained in the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
/// <typeparam name="TValue">The value type contained in the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
public class NetworkedDictionary<TKey, TValue> : INetworkable,
	ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
	IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	where TKey : INetworkable where TValue : INetworkable
{
	/// <inheritdoc/>
	public int NetworkId => 0;
	/// <inheritdoc/>
	public bool SupportEquals => false;
	/// <inheritdoc/>
	public bool SupportLerp => false;

	/// <summary>
	/// The underlying <see cref="Dictionary{TKey, TValue}"/> being contained inside.
	/// </summary>
	public Dictionary<TKey, TValue> Value
	{
		get => _value;
		set
		{
			_value = value;

			_changes.Clear();
			_changes.Add( (DictionaryChangeType.Clear, default( TKey ), default( TValue )) );
			foreach ( var (key, val) in value )
				_changes.Add( (DictionaryChangeType.Add, key, val) );
		}
	}
	/// <summary>
	/// See <see cref="Value"/>.
	/// </summary>
	private Dictionary<TKey, TValue> _value = null!;

	/// <summary>
	/// The indexer to the underlying <see cref="Dictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="key">The key to look at.</param>
	/// <returns>The value at that key.</returns>
	/// <exception cref="ArgumentNullException">key is null.</exception>
	public TValue this[TKey key]
	{
		get => Value[key];
	}

	/// <summary>
	/// The number of key/value pairs contained in the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	public int Count => Value.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <summary>
	/// A list of changes that have happened since the last time this was networked.
	/// </summary>
	private readonly List<(DictionaryChangeType, TKey?, TValue?)> _changes = new();

	/// <summary>
	/// Whether or not this <see cref="NetworkedHashSet{T}"/> is containing a key type that is a <see cref="ComplexNetworkable"/>.
	/// </summary>
	private readonly bool _containingKeyComplexNetworkable = typeof( TKey ).IsAssignableTo( typeof( ComplexNetworkable ) );

	/// <summary>
	/// Whether or not this <see cref="NetworkedHashSet{T}"/> is containing a value type that is a <see cref="ComplexNetworkable"/>.
	/// </summary>
	private readonly bool _containingValueComplexNetworkable = typeof( TValue ).IsAssignableTo( typeof( ComplexNetworkable ) );

	/// <summary>
	/// Initializes a default instance of <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	public NetworkedDictionary()
	{
		_value = new Dictionary<TKey, TValue>();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedDictionary{TKey, TValue}"/> with a pre-allocated <see cref="Dictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="dictionary">The pre-allocated <see cref="Dictionary{TKey, TValue}"/> to start with.</param>
	public NetworkedDictionary( Dictionary<TKey, TValue> dictionary )
	{
		_value = dictionary;
		foreach ( var (key, value) in dictionary )
			_changes.Add( (DictionaryChangeType.Add, key, value) );
	}

	/// <summary>
	/// Adds the specified key and value to the dictionary.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
	/// <exception cref="ArgumentNullException">key is null.</exception>
	/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="NetworkedDictionary{TKey, TValue}"/>.</exception>
	public void Add( TKey key, TValue value )
	{
		Value.Add( key, value );
		_changes.Add( (DictionaryChangeType.Add, key, value) );
	}

	/// <inheritdoc/>
	public void Add( KeyValuePair<TKey, TValue> pair )
	{
		Add( pair.Key, pair.Value );
	}

	/// <summary>
	/// Removes all keys and values from the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	public void Clear()
	{
		Value.Clear();
		_changes.Clear();
		_changes.Add( (DictionaryChangeType.Clear, default( TKey ), default( TValue )) );
	}

	/// <inheritdoc/>
	public bool Contains( KeyValuePair<TKey, TValue> pair )
	{
		return Value.ContainsKey( pair.Key ) && Value.ContainsValue( pair.Value );
	}

	/// <summary>
	/// Determines whether the <see cref="NetworkedDictionary{TKey, TValue}"/> contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the <see cref="NetworkedDictionary{TKey, TValue}"/>.</param>
	/// <returns>true if the <see cref="NetworkedDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">key is null.</exception>
	public bool ContainsKey( TKey key )
	{
		return Value.ContainsKey( key );
	}

	/// <summary>
	/// Removes the value with the specified key from the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the <see cref="NetworkedDictionary{TKey, TValue}"/>.</returns>
	/// <exception cref="ArgumentNullException">key is null.</exception>
	public bool Remove( TKey key )
	{
		var result = Value.Remove( key );
		if ( result )
			_changes.Add( (DictionaryChangeType.Remove, key, default( TValue )) );

		return result;
	}

	/// <inheritdoc/>
	public bool Remove( KeyValuePair<TKey, TValue> pair )
	{
		return Value.Remove( pair.Key );
	}

	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
	/// <returns>true if the <see cref="NetworkedDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">key is null.</exception>
	public bool TryGetValue( TKey key, [NotNullWhen( true )] out TValue? value )
	{
		return Value.TryGetValue( key, out value );
	}

	/// <summary>
	/// Copies the elements of the <see cref="NetworkedDictionary{TKey, TValue}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
	/// </summary>
	/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="NetworkedDictionary{TKey, TValue}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
	/// <exception cref="NotSupportedException">This method is not supported.</exception>
	public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc/>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return Value.GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedDictionary{TKey, TValue}"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedDictionary{TKey, TValue}"/> has changed.</returns>
	public bool Changed()
	{
		if ( _changes.Count > 0 )
			return true;

		foreach ( var (key, value) in Value )
		{
			if ( INetworkable.HasChanged( typeof( TKey ), key, key, !_containingKeyComplexNetworkable ) )
				return true;

			if ( INetworkable.HasChanged( typeof( TValue ), value, value, !_containingValueComplexNetworkable ) )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedDictionary{TKey, TValue}"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="NetworkedDictionary{TKey, TValue}"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue ) => false;

	/// <summary>
	/// Lerps a <see cref="NetworkedDictionary{TKey, TValue}"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="NetworkedDictionary{TKey, TValue}"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		Value = new Dictionary<TKey, TValue>();
		var dictionaryLength = reader.ReadInt32();
		for ( var i = 0; i < dictionaryLength; i++ )
		{
			TKey? key = default;
			TValue? value = default;

			if ( _containingKeyComplexNetworkable )
				key = (TKey)(INetworkable)ComplexNetworkable.GetById( reader.ReadInt32() )!;
			else
				key = reader.ReadNetworkable<TKey>();

			if ( _containingValueComplexNetworkable )
				value = (TValue)(INetworkable)ComplexNetworkable.GetById( reader.ReadInt32() )!;
			else
				value = reader.ReadNetworkable<TValue>();

			Add( key, value );
		}
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when a enumerated <see cref="DictionaryChangeType"/> is invalid.</exception>
	public void DeserializeChanges( NetworkReader reader )
	{
		var changeCount = reader.ReadInt32();
		for ( var i = 0; i < changeCount; i++ )
		{
			var changeType = (DictionaryChangeType)reader.ReadByte();
			switch ( changeType )
			{
				case DictionaryChangeType.Add:
					TKey key;
					if ( _containingKeyComplexNetworkable )
						key = (TKey)(INetworkable)ComplexNetworkable.GetById( reader.ReadInt32() )!;
					else
						key = reader.ReadNetworkable<TKey>();

					var hasValue = reader.ReadBoolean();
					if ( !hasValue )
					{
						Add( key, default! );
						break;
					}

					TValue? value;
					if ( _containingValueComplexNetworkable )
						value = (TValue)(INetworkable)ComplexNetworkable.GetById( reader.ReadInt32() )!;
					else
						value = reader.ReadNetworkable<TValue>();

					Add( key, value );
					break;
				case DictionaryChangeType.Remove:
					if ( _containingKeyComplexNetworkable )
						Remove( (TKey)(INetworkable)ComplexNetworkable.GetById( reader.ReadInt32() )! );
					else
						Remove( reader.ReadNetworkable<TKey>() );
					break;
				case DictionaryChangeType.Clear:
					Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( changeType ) );
			}
		}

		changeCount = reader.ReadInt32();
		if ( changeCount == 0 )
			return;

		var j = -1;
		var nextIndex = reader.ReadInt32();
		foreach ( var (key, value) in Value )
		{
			j++;
			if ( j != nextIndex )
				continue;

			if ( reader.ReadBoolean() )
				key.DeserializeChanges( reader );

			if ( reader.ReadBoolean() )
				value.DeserializeChanges( reader );
		}
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( Value.Count );
		foreach ( var (key, value) in Value )
		{
			if ( _containingKeyComplexNetworkable )
				writer.Write( key.NetworkId );
			else
				writer.Write( key );

			if ( _containingValueComplexNetworkable )
				writer.Write( value.NetworkId );
			else
				writer.Write( value );
		}
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when a enumerated <see cref="DictionaryChangeType"/> is invalid.</exception>
	public void SerializeChanges( NetworkWriter writer )
	{
		writer.Write( _changes.Count );
		foreach ( var (changeType, key, value) in _changes )
		{
			writer.Write( (byte)changeType );
			switch ( changeType )
			{
				case DictionaryChangeType.Add:
					if ( _containingKeyComplexNetworkable )
						writer.Write( key!.NetworkId );
					else
						writer.Write( key! );

					var hasValue = value is not null;
					writer.Write( hasValue );
					if ( !hasValue )
						break;

					if ( _containingValueComplexNetworkable )
						writer.Write( value!.NetworkId );
					else
						writer.Write( value! );
					break;
				case DictionaryChangeType.Remove:
					if ( _containingKeyComplexNetworkable )
						writer.Write( key!.NetworkId );
					else
						writer.Write( key! );
					break;
				case DictionaryChangeType.Clear:
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( changeType ) );
			}
		}
		_changes.Clear();

		if ( _containingKeyComplexNetworkable && _containingValueComplexNetworkable )
		{
			writer.Write( 0 );
			return;
		}

		var networkableCountPos = writer.BaseStream.Position;
		writer.BaseStream.Position += sizeof( int );
		var networkableChangeCount = 0;
		var i = -1;
		foreach ( var (key, value) in Value )
		{
			i++;
			var keyChanged = !INetworkable.HasChanged( typeof( TKey ), key, key, !_containingKeyComplexNetworkable );
			var valueChanged = !INetworkable.HasChanged( typeof( TValue ), value, value, !_containingValueComplexNetworkable );

			if ( !keyChanged && !valueChanged )
				continue;

			networkableChangeCount++;
			writer.Write( i );

			writer.Write( keyChanged );
			if ( keyChanged )
				key.SerializeChanges( writer );

			writer.Write( valueChanged );
			if ( valueChanged )
				value.SerializeChanges( writer );
		}

		var tempPos = writer.BaseStream.Position;
		writer.BaseStream.Position = networkableCountPos;
		writer.Write( networkableChangeCount );
		writer.BaseStream.Position = tempPos;
	}

	/// <summary>
	/// Returns the underlying <see cref="Dictionary{TKey, TValue}"/> contained in the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="networkedDictionary">The <see cref="NetworkedDictionary{TKey, TValue}"/> to get the <see cref="Dictionary{TKey, TValue}"/> from.</param>
	/// <returns>The underlying <see cref="Dictionary{TKey, TValue}"/> contained in the <see cref="NetworkedDictionary{TKey, TValue}"/>.</returns>
	public static implicit operator Dictionary<TKey, TValue>( NetworkedDictionary<TKey, TValue> networkedDictionary )
	{
		return networkedDictionary.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedDictionary{TKey, TValue}"/> that contains the provided <see cref="Dictionary{TKey, TValue}"/>.
	/// </summary>
	/// <param name="dictionary">The <see cref="Dictionary{TKey, TValue}"/> to contain in the <see cref="NetworkedDictionary{TKey, TValue}"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedDictionary{TKey, TValue}"/> that contains the provided <see cref="Dictionary{TKey, TValue}"/>.</returns>
	public static implicit operator NetworkedDictionary<TKey, TValue>( Dictionary<TKey, TValue> dictionary )
	{
		return new NetworkedDictionary<TKey, TValue>( dictionary );
	}

	/// <summary>
	/// Represents a type of change the <see cref="NetworkedDictionary{TKey, TValue}"/> has gone through.
	/// </summary>
	private enum DictionaryChangeType : byte
	{
		Add,
		Remove,
		Clear
	}
}
