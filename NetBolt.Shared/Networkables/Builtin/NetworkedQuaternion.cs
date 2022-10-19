using System;
using System.Numerics;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="Quaternion"/>.
/// </summary>
public struct NetworkedQuaternion : INetworkable, IEquatable<NetworkedQuaternion>
{
	/// <summary>
	/// The underlying <see cref="Quaternion"/> contained inside.
	/// </summary>
	public Quaternion Value
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
	private Quaternion _value;
	/// <summary>
	/// The last networked version of <see cref="Value"/>.
	/// </summary>
	private Quaternion _oldValue;

	/// <summary>
	/// The <see cref="Quaternion.X"/> component of the <see cref="Quaternion"/>.
	/// </summary>
	public float X => _value.X;
	/// <summary>
	/// The <see cref="Quaternion.Y"/> component of the <see cref="Quaternion"/>.
	/// </summary>
	public float Y => _value.Y;
	/// <summary>
	/// The <see cref="Quaternion.Z"/> component of the <see cref="Quaternion"/>.
	/// </summary>
	public float Z => _value.Z;
	/// <summary>
	/// The <see cref="Quaternion.W"/> component of the <see cref="Quaternion"/>.
	/// </summary>
	public float W => _value.W;

	/// <summary>
	/// Intializes a new instance of <see cref="NetworkedQuaternion"/> with the components of a <see cref="Quaternion"/>.
	/// </summary>
	/// <param name="x">The x component.</param>
	/// <param name="y">The y component.</param>
	/// <param name="z">The z component.</param>
	/// <param name="w">The w component.</param>
	public NetworkedQuaternion( float x, float y, float z, float w )
	{
		_value = new Quaternion( x, y, z, w );
		_oldValue = default;
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedQuaternion"/> with a pre-allocated <see cref="Quaternion"/>.
	/// </summary>
	/// <param name="quaternion">The value to initialize this instance with.</param>
	public NetworkedQuaternion( Quaternion quaternion )
	{
		_value = quaternion;
		_oldValue = default;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedQuaternion"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedQuaternion"/> has changed.</returns>
	public bool Changed()
	{
		return _value != _oldValue;
	}

	/// <summary>
	/// Lerps a <see cref="NetworkedQuaternion"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		var newQuaternion = (NetworkedQuaternion)newValue;
		if ( Value == (Quaternion)newQuaternion )
			return;


		Value = Quaternion.Lerp( (NetworkedQuaternion)oldValue, newQuaternion, fraction );
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		_value = new Quaternion( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() );
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void DeserializeChanges( NetworkReader reader )
	{
		Deserialize( reader );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void Serialize( NetworkWriter writer )
	{
		writer.Write( X );
		writer.Write( Y );
		writer.Write( Z );
		writer.Write( W );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		_oldValue = _value;
		Serialize( writer );
	}

	/// <summary>
	/// Indicates whether the current <see cref="NetworkedQuaternion"/> is equal to another <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <param name="other">An <see cref="NetworkedQuaternion"/> to compare with this <see cref="NetworkedQuaternion"/>.</param>
	/// <returns>true if the current <see cref="NetworkedQuaternion"/> is equal to the other <see cref="NetworkedQuaternion"/>; otherwise, false.</returns>
	public bool Equals( NetworkedQuaternion other )
	{
		return _value.Equals( other._value );
	}

	/// <inheritdoc/>
	public override bool Equals( object? obj )
	{
		return obj is NetworkedVector3 other && Equals( other );
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return _value.GetHashCode();
	}

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkedQuaternion"/>.</returns>
	public override string ToString()
	{
		return Value.ToString();
	}

	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedQuaternion"/>s are equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedQuaternion"/>s are equal.</returns>
	public static bool operator ==( NetworkedQuaternion left, NetworkedQuaternion right ) => left.Equals( right );
	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedQuaternion"/>s are not equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedQuaternion"/>s are not equal.</returns>
	public static bool operator !=( NetworkedQuaternion left, NetworkedQuaternion right ) => !(left == right);

	/// <summary>
	/// Returns the underlying <see cref="Quaternion"/> in a <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <param name="networkedQuaternion">The <see cref="NetworkedQuaternion"/> to get the <see cref="Quaternion"/> from.</param>
	/// <returns>The underlying <see cref="Quaternion"/> contained in the <see cref="NetworkedQuaternion"/>.</returns>
	public static implicit operator Quaternion( NetworkedQuaternion networkedQuaternion )
	{
		return networkedQuaternion.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedQuaternion"/> that contains the provided <see cref="Quaternion"/>.
	/// </summary>
	/// <param name="quaternion">The <see cref="Quaternion"/> to contain in the <see cref="NetworkedQuaternion"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedQuaternion"/> that contains the provided <see cref="Quaternion"/>.</returns>
	public static implicit operator NetworkedQuaternion( Quaternion quaternion )
	{
		return new NetworkedQuaternion( quaternion );
	}
}
