using NetBolt.Shared.Clients;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A bi-directional <see cref="NetworkMessage"/> defining a message from a client.
/// </summary>
public sealed class ClientSayMessage : NetworkMessage
{
	/// <summary>
	/// The <see cref="INetworkClient"/> that has changed.
	/// </summary>
	public INetworkClient? Client { get; private set; }
	/// <summary>
	/// The new state of the <see cref="Client"/>.
	/// </summary>
	public string Message { get; private set; } = "";

	/// <summary>
	/// Initializes a new instance of <see cref="ClientSayMessage"/> with the client who created the message and the message that was sent.
	/// </summary>
	/// <param name="client">The client that created the message.</param>
	/// <param name="message">The message the client sent.</param>
	public ClientSayMessage( INetworkClient client, string message )
	{
		Client = client;
		Message = message;
	}

	/// <summary>
	/// Initializes a default instance of <see cref="ClientSayMessage"/>.
	/// </summary>
	public ClientSayMessage()
	{
		Client = null;
		Message = string.Empty;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientSayMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		Client = INetworkClient.GetClientById( reader.ReadInt64() );
		Message = reader.ReadString();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientSayMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( Client!.ClientId );
		writer.Write( Message );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="ClientSayMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="ClientSayMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( ClientSayMessage );
	}
}
