using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A <see cref="NetworkMessage"/> that notifies clients that the server is shutting down.
/// </summary>
public sealed class ShutdownMessage : NetworkMessage
{
	/// <summary>
	/// Deserializes all information relating to the <see cref="ShutdownMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ShutdownMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
	}
}
