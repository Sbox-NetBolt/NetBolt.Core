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
	/// Initializes a new instance of <see cref="NetworkedQuaternion"/>.
	/// </summary>
	/// <param name="quaternion">The value to initialize this instance with.</param>
	private NetworkedQuaternion( Quaternion quaternion )
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

	/// <summary>
	/// Indicates whether this instance and a specified object are equal.
	/// </summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
	public override bool Equals( object? obj )
	{
		return obj is NetworkedVector3 other && Equals( other );
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

#if CLIENT
	/// <summary>
	/// Returns the underlying <see cref="Quaternion"/> casted to a <see cref="Rotation"/>.
	/// </summary>
	/// <param name="networkedQuaternion">The <see cref="NetworkedQuaternion"/> to get the <see cref="Quaternion"/> from.</param>
	/// <returns>The underlying <see cref="Quaternion"/> casted to a <see cref="Rotation"/>.</returns>
	public static implicit operator Rotation( NetworkedQuaternion networkedQuaternion )
	{
		return networkedQuaternion.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedQuaternion"/> that contains the provided <see cref="Rotation"/> as a <see cref="Quaternion"/>.
	/// </summary>
	/// <param name="rotation">The <see cref="Rotation"/> to contain in the <see cref="NetworkedQuaternion"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedQuaternion"/> that contains the provided <see cref="Rotation"/> as a <see cref="Quaternion"/>.</returns>
	public static implicit operator NetworkedQuaternion( Rotation rotation )
	{
		return new NetworkedQuaternion( new Quaternion( rotation.x, rotation.y, rotation.z, rotation.w ) );
	}
#endif

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
