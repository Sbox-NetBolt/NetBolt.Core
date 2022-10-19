using NetBolt.Shared.Clients;
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
	public INetworkClient Client { get; private set; } = null!;
	/// <summary>
	/// The new state of the <see cref="Client"/>.
	/// </summary>
	public ClientState ClientState { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	[ClientOnly]
	public ClientStateChangedMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ClientStateChangedMessage"/> with the <see cref="INetworkClient"/> and their new state.
	/// </summary>
	/// <param name="client">The <see cref="INetworkClient"/> that has changed.</param>
	/// <param name="clientState">The new state of the client.</param>
	[ServerOnly]
	public ClientStateChangedMessage( INetworkClient client, ClientState clientState )
	{
		Client = client;
		ClientState = clientState;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		var clientId = reader.ReadInt64();
		var client = INetworkClient.GetClientById( clientId );
		if ( client is null )
			client = reader.ReadBoolean() ? IGlue.Instance.GetBot( clientId ) : IGlue.Instance.GetClient( clientId );
		else
			_ = reader.ReadBoolean();

		Client = client!;
		ClientState = (ClientState)reader.ReadByte();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( Client.ClientId );
		writer.Write( Client.IsBot );
		writer.Write( (byte)ClientState );
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
