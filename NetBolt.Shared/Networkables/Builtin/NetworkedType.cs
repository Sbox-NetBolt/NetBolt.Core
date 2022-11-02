using NetBolt.Shared.Utility;
using System;

namespace NetBolt.Shared.Networkables.Builtin;

// TODO: Need to support generics in types.
/// <summary>
/// Represents a networkable <see cref="Type"/>.
/// </summary>
public class NetworkedType : INetworkable, IEquatable<NetworkedType>
{
	/// <inheritdoc/>
	public int NetworkId => 0;
	/// <inheritdoc/>
	public bool SupportEquals => true;
	/// <inheritdoc/>
	public bool SupportLerp => false;

	/// <summary>
	/// The underlying <see cref="Type"/> contained inside.
	/// </summary>
	public Type Value
	{
		get => _value;
		set
		{
			_value = value;
			_changed = true;
		}
	}
	/// <summary>
	/// See <see cref="Type"/>.
	/// </summary>
	private Type _value = null!;

	/// <summary>
	/// Whether or not the <see cref="Value"/> has changed.
	/// </summary>
	private bool _changed = false;

	/// <summary>
	/// Initializes a default instance of <see cref="NetworkedType"/>.
	/// </summary>
	public NetworkedType()
	{
		_value = typeof( object );
	}

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkedType"/>.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> to initialize this instance with.</param>
	public NetworkedType( Type type )
	{
		Value = type;
	}

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedType"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="NetworkedType"/> has changed.</returns>
	public bool Changed() => _changed;

	/// <summary>
	/// Returns whether or not the <see cref="NetworkedType"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="NetworkedType"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue ) => Equals( oldValue as NetworkedType );

	/// <summary>
	/// Lerps a <see cref="NetworkedType"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="NetworkedType"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void Deserialize( NetworkReader reader )
	{
		var typeName = reader.ReadString();
		var type = ITypeLibrary.Instance.GetTypeByName( typeName );

		if ( type is not null )
			Value = type;
		else
		{
			Value = typeof( object );
			ILogger.Instance.Error( "No type with the name \"{0}\" exists", typeName );
		}
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
		// TODO: Use FullName once S&box supports finding types by full name. https://github.com/Facepunch/sbox-issues/issues/2413
		writer.Write( Value.Name );
	}

	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkedString"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		Serialize( writer );
		_changed = false;
	}

	/// <summary>
	/// Indicates whether the current <see cref="NetworkedType"/> is equal to another <see cref="NetworkedType"/>.
	/// </summary>
	/// <param name="other">An <see cref="NetworkedType"/> to compare with this <see cref="NetworkedType"/>.</param>
	/// <returns>true if the current <see cref="NetworkedType"/> is equal to the other <see cref="NetworkedType"/>; otherwise, false.</returns>
	public bool Equals( NetworkedType? other )
	{
		if ( other is null )
			return false;

		return Value == other.Value;
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
	/// Returns a string that represents the <see cref="NetworkedType"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkedType"/>.</returns>
	public override string ToString()
	{
		return Value.ToString();
	}

	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedType"/>s are equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedType"/>s are equal.</returns>
	public static bool operator ==( NetworkedType left, NetworkedType right ) => left.Value == right.Value;
	/// <summary>
	/// Returns whether or not the two <see cref="NetworkedType"/>s are not equal.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>Whether or not the two <see cref="NetworkedType"/>s are not equal.</returns>
	public static bool operator !=( NetworkedType left, NetworkedType right ) => !(left.Value == right.Value);

	/// <summary>
	/// Returns the underlying <see cref="Type"/> contained in the <see cref="NetworkedType"/>.
	/// </summary>
	/// <param name="networkedType">The <see cref="NetworkedType"/> to get the <see cref="Type"/> from.</param>
	/// <returns>The underlying <see cref="Type"/> contained in the <see cref="NetworkedType"/>.</returns>
	public static implicit operator Type( NetworkedType networkedType )
	{
		return networkedType.Value;
	}

	/// <summary>
	/// Returns a new <see cref="NetworkedType"/> that contains the provided <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> to contain in the <see cref="NetworkedType"/>.</param>
	/// <returns>A new instance of <see cref="NetworkedType"/> that contains the provided <see cref="Type"/>.</returns>
	public static implicit operator NetworkedType( Type type )
	{
		return new NetworkedType( type );
	}
}
