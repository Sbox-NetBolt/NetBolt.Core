using System;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="NetworkedString"/>.
/// </summary>
public struct NetworkedString : INetworkable, IEquatable<NetworkedString>
{
	/// <summary>
	/// The underlying <see cref="string"/> contained inside.
	/// </summary>
	public string Value
	{
		get => _value;
		set
		{
			_oldValue = _value;
			_value = value;
		}
	}
	/// <summary>
	/// See <see cref="Value"/>.
	/// </summary>
	private string _value;
	/// <summary>
	/// The last networked version of <see cref="Value"/>.
	/// </summary>
	private string _oldValue;

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="s">The value to initialize this instance with.</param>
	private NetworkedString( string s )
	{
		_value = s;
		_oldValue = string.Empty;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedString"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedString"/> has changed.</returns>
	public bool Changed()
	{
		return _value != _oldValue;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		_value = reader.ReadString();
	}
	
	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void DeserializeChanges( NetworkReader reader )
	{
		Deserialize( reader );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( _value );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		_oldValue = _value;
		Serialize( writer );
	}

	/// <summary>
	/// Indicates whether the current <see cref="NetworkedString"/> is equal to another <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="other">An <see cref="NetworkedString"/> to compare with this <see cref="NetworkedString"/>.</param>
	/// <returns>true if the current <see cref="NetworkedString"/> is equal to the other <see cref="NetworkedString"/>; otherwise, false.</returns>
	public bool Equals( NetworkedString other )
	{
		return _value == other._value;
	}

	/// <summary>
	/// Indicates whether this instance and a specified object are equal.
	/// </summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
	public override bool Equals( object? obj )
	{
		return obj is NetworkedString other && Equals( other );
	}

	/// <summary>
	/// Returns the hash code for this instance.
	/// </summary>
	/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkedString"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkedString"/>.</returns>
	public override string ToString()
	{
		return Value;
	}

	/// <summary>
	/// Concatenates two <see cref="NetworkedString"/>s together.
	/// </summary>
	/// <param name="left">The left side of the new <see cref="NetworkedString"/>.</param>
	/// <param name="right">The right side of the new <see cref="NetworkedString"/>.</param>
	/// <returns>The new <see cref="NetworkedString"/>.</returns>
	public static NetworkedString operator +( NetworkedString left, NetworkedString right ) => left.Value + right.Value;

	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedString"/>s are the same.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedString"/>s are the same.</returns>
	public static bool operator ==( NetworkedString left, NetworkedString right ) => left.Value == right.Value;
	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedString"/>s are not the same.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedString"/>s are the same.</returns>
	public static bool operator !=( NetworkedString left, NetworkedString right ) => !(left == right);

	/// <summary>
	/// Returns the underlying <see cref="string"/> contained in the <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="networkedString">The <see cref="NetworkedString"/> to get the <see cref="string"/> from.</param>
	/// <returns>The underlying <see cref="string"/> contained in the <see cref="NetworkedString"/>.</returns>
	public static implicit operator string( NetworkedString networkedString )
	{
		return networkedString.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedString"/> that contains the provided <see cref="string"/>.
	/// </summary>
	/// <param name="s">The <see cref="string"/> to contain in the <see cref="NetworkedString"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedString"/> that contains the provided <see cref="string"/>.</returns>
	public static implicit operator NetworkedString( string s )
	{
		return new NetworkedString( s );
	}
}
