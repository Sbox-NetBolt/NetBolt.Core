using System;
using System.Numerics;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="System.Numerics.Vector3"/>.
/// </summary>
public struct NetworkedVector3 : INetworkable, IEquatable<NetworkedVector3>
{
	/// <inheritdoc/>
	public int NetworkId => 0;
	/// <inheritdoc/>
	public bool SupportEquals => true;
	/// <inheritdoc/>
	public bool SupportLerp => true;

	/// <summary>
	/// The underlying <see cref="System.Numerics.Vector3"/> contained inside.
	/// </summary>
	public System.Numerics.Vector3 Value
	{
		get => _value;
		set
		{
			_value = value;
			_changed = true;
		}
	}
	/// <summary>
	/// See <see cref="Value"/>.
	/// </summary>
	private System.Numerics.Vector3 _value;
	/// <summary>
	/// Whether or not the <see cref="NetworkedVector3"/> has changed.
	/// </summary>
	private bool _changed = false;

	/// <summary>
	/// The <see cref="System.Numerics.Vector3.X"/> component of the <see cref="System.Numerics.Vector3"/>.
	/// </summary>
	public float X
	{
		get => _value.X;
		set
		{
			_value.X = value;
			_changed = true;
		}
	}
	/// <summary>
	/// The <see cref="System.Numerics.Vector3.Y"/> component of the <see cref="System.Numerics.Vector3"/>.
	/// </summary>
	public float Y
	{
		get => _value.Y;
		set
		{
			_value.Y = value;
			_changed = true;
		}
	}
	/// <summary>
	/// The <see cref="System.Numerics.Vector3.Z"/> component of the <see cref="System.Numerics.Vector3"/>.
	/// </summary>
	public float Z
	{
		get => _value.Z;
		set
		{
			_value.Z = value;
			_changed = true;
		}
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedVector3"/> from a <see cref="System.Numerics.Vector3"/>s components.
	/// </summary>
	/// <param name="x">The x value.</param>
	/// <param name="y">The y value.</param>
	/// <param name="z">The z value.</param>
	public NetworkedVector3( float x, float y, float z )
	{
		var vector3 = new System.Numerics.Vector3( x, y, z );
		_value = vector3;
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="vector3">The value to initialize this instance with.</param>
	public NetworkedVector3( System.Numerics.Vector3 vector3 )
	{
		_value = vector3;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedVector3"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedVector3"/> has changed.</returns>
	public bool Changed()
	{
		return _changed;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedVector3"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="NetworkedVector3"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue )
	{
		if ( oldValue is not NetworkedVector3 networkedVector3 )
			return false;

		return Equals( networkedVector3 );
	}

	/// <summary>
	/// Lerps a <see cref="NetworkedVector3"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		var newVector3 = (NetworkedVector3)newValue;
		if ( System.Numerics.Vector3.DistanceSquared( Value, newVector3 ) < 1 )
			return;


		Value = Vector3.Lerp( ((NetworkedVector3)oldValue).Value, newVector3.Value, fraction );
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		_value = reader.ReadVector3();
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void DeserializeChanges( NetworkReader reader )
	{
		Deserialize( reader );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( _value );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		Serialize( writer );
		_changed = false;
	}

	/// <summary>
	/// Indicates whether the current <see cref="NetworkedVector3"/> is equal to another <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="other">An <see cref="NetworkedVector3"/> to compare with this <see cref="NetworkedVector3"/>.</param>
	/// <returns>true if the current <see cref="NetworkedVector3"/> is equal to the other <see cref="NetworkedVector3"/>; otherwise, false.</returns>
	public bool Equals( NetworkedVector3 other )
	{
		return Value.Equals( other.Value );
	}

	/// <inheritdoc/>
	public override bool Equals( object? obj )
	{
		return obj is NetworkedVector3 other && Equals( other );
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkedString"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkedString"/>.</returns>
	public override string ToString()
	{
		return $"{X:0.####}, {Y:0.####}, {Z:0.####}";
	}

	/// <summary>
	/// Adds two <see cref="NetworkedVector3"/>s together.
	/// </summary>
	/// <param name="left">The left operand to add.</param>
	/// <param name="right">The right operand to add.</param>
	/// <returns>The result of the addition.</returns>
	public static NetworkedVector3 operator +( NetworkedVector3 left, NetworkedVector3 right ) => left.Value + right.Value;
	/// <summary>
	/// Flips the sign on each component of the <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="operand">The operand to flip.</param>
	/// <returns>The result of the flip.</returns>
	public static NetworkedVector3 operator -( NetworkedVector3 operand ) => -operand.Value;
	/// <summary>
	/// Subtracts the right <see cref="NetworkedVector3"/> from the left <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="left">The operand to subtract from.</param>
	/// <param name="right">The operand to subtract.</param>
	/// <returns>The result of the subtraction.</returns>
	public static NetworkedVector3 operator -( NetworkedVector3 left, NetworkedVector3 right ) => left.Value - right.Value;
	/// <summary>
	/// Multiplies a <see cref="NetworkedVector3"/> by a scalar value.
	/// </summary>
	/// <param name="left">The <see cref="NetworkedVector3"/> to multiply.</param>
	/// <param name="mult">The scalar value to multiply by.</param>
	/// <returns>The result of the multiplication.</returns>
	public static NetworkedVector3 operator *( NetworkedVector3 left, float mult ) => left.Value * mult;
	/// <summary>
	/// Multiples a <see cref="NetworkedVector3"/> by a scalar value.
	/// </summary>
	/// <param name="mult">The scalar value to multiply by.</param>
	/// <param name="right">The <see cref="NetworkedVector3"/> to multiply.</param>
	/// <returns>The result of the multiplication.</returns>
	public static NetworkedVector3 operator *( float mult, NetworkedVector3 right ) => mult * right.Value;
	/// <summary>
	/// Multiplies the left <see cref="NetworkedVector3"/> by the <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="left">The left operand to multiply.</param>
	/// <param name="right">The right operand to multiply by.</param>
	/// <returns>The result of the multiplication.</returns>
	public static NetworkedVector3 operator *( NetworkedVector3 left, NetworkedVector3 right ) => left.Value * right.Value;
	/// <summary>
	/// Divides a <see cref="NetworkedVector3"/> by a scalar value.
	/// </summary>
	/// <param name="left">The <see cref="NetworkedVector3"/> to divide.</param>
	/// <param name="mult">The scalar value to divide by.</param>
	/// <returns>The result of the divide operation.</returns>
	public static NetworkedVector3 operator /( NetworkedVector3 left, float mult ) => left.Value / mult;
	/// <summary>
	/// Divides the left <see cref="NetworkedVector3"/> by the right <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="left">The left operand to divide.</param>
	/// <param name="right">The right operand to divide by.</param>
	/// <returns>The result of the divide operation.</returns>
	public static NetworkedVector3 operator /( NetworkedVector3 left, NetworkedVector3 right ) => left.Value / right.Value;

	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedVector3"/>s are equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedVector3"/>s are equal.</returns>
	public static bool operator ==( NetworkedVector3 left, NetworkedVector3 right ) => left.Value == right.Value;
	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedVector3"/>s are not equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedVector3"/>s are not equal.</returns>
	public static bool operator !=( NetworkedVector3 left, NetworkedVector3 right ) => !(left.Value == right.Value);

	/// <summary>
	/// Returns the underlying <see cref="System.Numerics.Vector3"/> in a <see cref="NetworkedVector3"/>.
	/// </summary>
	/// <param name="networkedVector3">The <see cref="NetworkedVector3"/> to get the <see cref="System.Numerics.Vector3"/> from.</param>
	/// <returns>The underlying <see cref="System.Numerics.Vector3"/> contained in the <see cref="NetworkedVector3"/>.</returns>
	public static implicit operator System.Numerics.Vector3( NetworkedVector3 networkedVector3 )
	{
		return networkedVector3.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedVector3"/> that contains the provided <see cref="System.Numerics.Vector3"/>.
	/// </summary>
	/// <param name="vector3">The <see cref="System.Numerics.Vector3"/> to contain in the <see cref="NetworkedVector3"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedVector3"/> that contains the provided <see cref="System.Numerics.Vector3"/>.</returns>
	public static implicit operator NetworkedVector3( System.Numerics.Vector3 vector3 )
	{
		return new NetworkedVector3( vector3 );
	}
}
