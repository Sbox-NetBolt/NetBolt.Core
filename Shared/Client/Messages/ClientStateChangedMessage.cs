using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a client ID and the state it is now in.
/// </summary>
public sealed class ClientStateChangedMessage : NetworkMessage
{
	/// <summary>
	/// The <see cref="INetworkClient"/> that has changed.
	/// </summary>
	public INetworkClient Client { get; private set; }
	/// <summary>
	/// The new state of the <see cref="Client"/>.
	/// </summary>
	public ClientState ClientState { get; private set; }

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="ClientStateChangedMessage"/> with the <see cref="INetworkClient"/> and their new state.
	/// </summary>
	/// <param name="client">The <see cref="INetworkClient"/> that has changed.</param>
	/// <param name="clientState">The new state of the client.</param>
	public ClientStateChangedMessage( INetworkClient client, ClientState clientState )
	{
		Client = client;
		ClientState = clientState;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		var clientId = reader.ReadInt64();
		var client = INetworkClient.GetClientById( clientId );
		if ( client is null )
			client = reader.ReadBoolean() ? new BotClient( clientId ) : new NetworkClient( clientId );
		else
			_ = reader.ReadBoolean();

		Client = client!;
		ClientState = (ClientState)reader.ReadByte();
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( Client.ClientId );
		writer.Write( Client.IsBot );
		writer.Write( (byte)ClientState );
#endif
	}

	/// <summary>
	/// Returns a string that represents the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="ClientStateChangedMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( ClientStateChangedMessage );
	}
}
