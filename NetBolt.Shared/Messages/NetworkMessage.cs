using System;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// Base class for sending any information between a client and a server.
/// </summary>
public abstract class NetworkMessage : INetworkable
{
	/// <inheritdoc/>
	public int NetworkId => 0;
	/// <inheritdoc/>
	public bool SupportEquals => false;
	/// <inheritdoc/>
	public bool SupportLerp => false;

	/// <summary>
	/// Network messages can never "change"
	/// </summary>
	/// <returns>False</returns>
	public bool Changed() => false;

	/// <summary>
	/// Returns whether or not the <see cref="NetworkMessage"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="NetworkMessage"/> instance is the same as another.</returns>
	public bool Equals( INetworkable? oldValue ) => false;

	/// <summary>
	/// Lerps a <see cref="NetworkMessage"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	/// <exception cref="NotSupportedException">Lerping a <see cref="NetworkMessage"/> is not supported.</exception>
	public void Lerp( float fraction, INetworkable oldValue, INetworkable newValue )
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="NetworkMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public abstract void Deserialize( NetworkReader reader );
	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public void DeserializeChanges( NetworkReader reader )
	{
		Deserialize( reader );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="NetworkMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public abstract void Serialize( NetworkWriter writer );
	/// <summary>
	/// Serializes all changes relating to the <see cref="NetworkMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public void SerializeChanges( NetworkWriter writer )
	{
		Serialize( writer );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="NetworkMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( NetworkMessage );
	}

	/// <summary>
	/// Deserializes a <see cref="NetworkMessage"/> from a <see cref="NetworkReader"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	/// <returns>The deserialized <see cref="NetworkMessage"/>.</returns>
	public static NetworkMessage DeserializeMessage( NetworkReader reader )
	{
		return reader.ReadNetworkable<NetworkMessage>();
	}
}
