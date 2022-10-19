using NetBolt.Shared.Utility;
using System;

namespace NetBolt.Shared.Networkables.Builtin;

/// <summary>
/// Represents a networkable <see cref="Type"/>.
/// </summary>
public class NetworkedType : INetworkable
{
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
		var type = IGlue.Instance.TypeLibrary.GetTypeByName( typeName );

		if ( type is not null )
			Value = type;
		else
		{
			Value = typeof( object );
			IGlue.Instance.Logger.Error( "No type with the name \"{0}\" exists", typeName );
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
