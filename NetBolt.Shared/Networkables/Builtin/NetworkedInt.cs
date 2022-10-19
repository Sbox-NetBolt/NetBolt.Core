using System;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

// TODO: In .NET 7 make this generic where T : INumber https://devblogs.microsoft.com/dotnet/dotnet-7-generic-math/
/// <summary>
/// Represents a networkable <see cref="int"/>.
/// </summary>
public struct NetworkedInt : INetworkable, IEquatable<NetworkedInt>
{
	/// <summary>
	/// The underlying <see cref="int"/> being contained inside.
	/// </summary>
	public int Value
	{
		get => _value;
		set
		{
			_oldValue = _value;
			_value = value;
		}
	}
	/// <summary>
	/// <see cref="Value"/>.
	/// </summary>
	private int _value;
	/// <summary>
	/// The last networked version of <see cref="Value"/>.
	/// </summary>
	private int _oldValue;

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="i">The value to initialize this instance with.</param>
	private NetworkedInt( int i )
	{
		_value = i;
		_oldValue = default;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedInt"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedInt"/> has changed.</returns>
	public bool Changed()
	{
		return _value != _oldValue;
	}

	/// <summary>
	/// Lerps a <see cref="NetworkedInt"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		var newInt = (NetworkedInt)newValue;
		if ( Value == newInt )
			return;

		Value = (int)((NetworkedInt)oldValue + (newInt - (NetworkedInt)oldValue) * fraction);
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		_value = reader.ReadInt32();
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void DeserializeChanges( NetworkReader reader )
	{
		Deserialize( reader );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( _value );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		_oldValue = _value;
		Serialize( writer );
	}

	/// <summary>
	/// Indicates whether the current <see cref="NetworkedInt"/> is equal to another <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="other">An <see cref="NetworkedInt"/> to compare with this <see cref="NetworkedInt"/>.</param>
	/// <returns>true if the current <see cref="NetworkedInt"/> is equal to the other <see cref="NetworkedInt"/>; otherwise, false.</returns>
	public bool Equals( NetworkedInt other )
	{
		return _value == other._value;
	}

	/// <inheritdoc/>
	public override bool Equals( object? obj )
	{
		return obj is NetworkedInt other && Equals( other );
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return _value;
	}

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkedInt"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkedInt"/>.</returns>
	public override string ToString()
	{
		return Value.ToString();
	}

	/// <summary>
	/// Uses the + operation on a single <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="operand">The operand to use the + on.</param>
	/// <returns>The same operand.</returns>
	public static NetworkedInt operator +( NetworkedInt operand ) => operand;
	/// <summary>
	/// Adds two <see cref="NetworkedInt"/>s together.
	/// </summary>
	/// <param name="left">The left operand to add.</param>
	/// <param name="right">The right operand to add.</param>
	/// <returns>The result of the addition.</returns>
	public static NetworkedInt operator +( NetworkedInt left, NetworkedInt right ) => left.Value + right.Value;
	/// <summary>
	/// Adds one (1) to the operand.
	/// </summary>
	/// <param name="operand">The operand to add one (1) to.</param>
	/// <returns>The result of the operation.</returns>
	public static NetworkedInt operator ++( NetworkedInt operand ) => operand.Value + 1;
	/// <summary>
	/// Flips the sign on an operand.
	/// </summary>
	/// <param name="operand">The operand to flip.</param>
	/// <returns>The result of the flip.</returns>
	public static NetworkedInt operator -( NetworkedInt operand ) => -operand.Value;
	/// <summary>
	/// Subtracts the right <see cref="NetworkedInt"/> from the left <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="left">The operand to subtract from.</param>
	/// <param name="right">The operand to subtract.</param>
	/// <returns>The result of the subtraction.</returns>
	public static NetworkedInt operator -( NetworkedInt left, NetworkedInt right ) => left.Value - right.Value;
	/// <summary>
	/// Subtracts one (1) from the operand.
	/// </summary>
	/// <param name="operand">The operand to subtract one (1) from.</param>
	/// <returns>The result of the operation.</returns>
	public static NetworkedInt operator --( NetworkedInt operand ) => operand.Value - 1;
	/// <summary>
	/// Multiplies the left <see cref="NetworkedInt"/> by the right <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="left">The left operand to multiply.</param>
	/// <param name="right">The right operand to multiply by.</param>
	/// <returns>The result of the multiplication.</returns>
	public static NetworkedInt operator *( NetworkedInt left, NetworkedInt right ) => left.Value * right.Value;
	/// <summary>
	/// Divides the left <see cref="NetworkedInt"/> by the right <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="left">The left operand to divide.</param>
	/// <param name="right">The right operand to divide by.</param>
	/// <returns>The result of the divide operation.</returns>
	public static NetworkedInt operator /( NetworkedInt left, NetworkedInt right ) => left.Value / right.Value;
	/// <summary>
	/// Returns the integer division result of the two operands.
	/// </summary>
	/// <param name="left">The left operand to divide.</param>
	/// <param name="right">The right operand to divide by.</param>
	/// <returns>The result of the modulo operation.</returns>
	public static NetworkedInt operator %( NetworkedInt left, NetworkedInt right ) => left.Value % right.Value;

	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedInt"/>s are equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedInt"/>s are equal.</returns>
	public static bool operator ==( NetworkedInt left, NetworkedInt right ) => left.Value == right.Value;
	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedInt"/> are not equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedInt"/>s are not equal.</returns>
	public static bool operator !=( NetworkedInt left, NetworkedInt right ) => !(left == right);
	/// <summary>
	/// Returns whether or not the left operand is less than the right operand.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the left operand is less than the right operand.</returns>
	public static bool operator <( NetworkedInt left, NetworkedInt right ) => left.Value < right.Value;
	/// <summary>
	/// Returns whether or not the left operand is greater than the right operand.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the left operand is greater than the right operand.</returns>
	public static bool operator >( NetworkedInt left, NetworkedInt right ) => left.Value > right.Value;
	/// <summary>
	/// Returns whether or not the left operand is less than or equal to the right operand.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the left operand is less than or equal to the right operand.</returns>
	public static bool operator <=( NetworkedInt left, NetworkedInt right ) => left.Value <= right.Value;
	/// <summary>
	/// Returns whether or not the left operand is greater than or equal to the right operand.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the left operand is greater than or equal to the right operand.</returns>
	public static bool operator >=( NetworkedInt left, NetworkedInt right ) => left.Value >= right.Value;

	/// <summary>
	/// Returns the underlying <see cref="int"/> contained in the <see cref="NetworkedInt"/>.
	/// </summary>
	/// <param name="networkedInt">The <see cref="NetworkedInt"/> to get the value from.</param>
	/// <returns>The underlying <see cref="int"/> contained in the <see cref="NetworkedInt"/>.</returns>
	public static implicit operator int( NetworkedInt networkedInt )
	{
		return networkedInt.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedInt"/> that contains the provided <see cref="int"/>.
	/// </summary>
	/// <param name="i">The <see cref="int"/> to contain in the <see cref="NetworkedInt"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedInt"/> that contains the provided <see cref="int"/>.</returns>
	public static implicit operator NetworkedInt( int i )
	{
		return new NetworkedInt( i );
	}
}
