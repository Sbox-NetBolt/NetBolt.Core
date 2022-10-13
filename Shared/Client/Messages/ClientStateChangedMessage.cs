using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a client ID and the state it is now in.
/// </summary>
public sealed class ClientStateChangedMessage : NetworkMessage
{
	/// <summary>
	/// The ID of the client that has changed.
	/// </summary>
	public long ClientId { get; private set; }
	/// <summary>
	/// The new state of the client.
	/// </summary>
	public ClientState ClientState { get; private set; }

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="ClientStateChangedMessage"/> with the unique client identifier and their new state.
	/// </summary>
	/// <param name="clientId">The unique identifier of the client that has changed.</param>
	/// <param name="clientState">The new state of the client.</param>
	public ClientStateChangedMessage( long clientId, ClientState clientState )
	{
		ClientId = clientId;
		ClientState = clientState;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		ClientId = reader.ReadInt64();
		ClientState = (ClientState)reader.ReadByte();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( ClientId );
		writer.Write( (byte)ClientState );
	}
	
	/// <summary>
	/// Returns a string that represents the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="ClientStateChangedMessage"/>.</returns>
	public override string ToString()
	{
		return nameof(ClientStateChangedMessage);
	}
}
