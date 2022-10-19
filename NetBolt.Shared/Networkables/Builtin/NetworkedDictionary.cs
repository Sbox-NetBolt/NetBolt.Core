using NetBolt.Shared.Utility;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The key type contained in the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
/// <typeparam name="TValue">The value type contained in the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
public class NetworkedDictionary<TKey, TValue> : INetworkable, IEnumerable<KeyValuePair<TKey, TValue>> where TKey : INetworkable where TValue : INetworkable
{
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
	/// The number of key/value pairs contained in the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	public int Count => Value.Count;

	/// <summary>
	/// The list of changes that have happened since the last time this was networked.
	/// </summary>
	private readonly List<(DictionaryChangeType, TKey?, TValue?)> _changes = new();

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

	/// <summary>
	/// Removes all keys and values from the <see cref="NetworkedDictionary{TKey, TValue}"/>.
	/// </summary>
	public void Clear()
	{
		Value.Clear();
		_changes.Clear();
		_changes.Add( (DictionaryChangeType.Clear, default( TKey ), default( TValue )) );
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
		return _changes.Count > 0;
	}

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
			Value.Add( reader.ReadNetworkable<TKey>(), reader.ReadNetworkable<TValue>() );
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
					var key = reader.ReadNetworkable<TKey>();

					var hasValue = reader.ReadBoolean();
					TValue? value = default;
					if ( hasValue )
						value = reader.ReadNetworkable<TValue>();

					Add( key, value! );
					break;
				case DictionaryChangeType.Remove:
					Remove( reader.ReadNetworkable<TKey>() );
					break;
				case DictionaryChangeType.Clear:
					Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( changeType ) );
			}
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
			writer.WriteNetworkable( key );
			writer.WriteNetworkable( value );
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
			switch( changeType )
			{
				case DictionaryChangeType.Add:
					writer.WriteNetworkable( key! );

					var hasValue = value is not null;
					writer.Write( hasValue );
					if ( hasValue )
						writer.WriteNetworkable( value! );
					break;
				case DictionaryChangeType.Remove:
					writer.WriteNetworkable( key! );
					break;
				case DictionaryChangeType.Clear:
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( changeType ) );
			}
		}
		_changes.Clear();
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
