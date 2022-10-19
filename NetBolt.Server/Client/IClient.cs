using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.WebSocket;

namespace NetBolt.Server;

/// <summary>
/// A contract to define a client in a server.
/// </summary>
public interface IClient : INetworkClient, IWebSocketClient
{
	/// <summary>
	/// Serializes a message and sends the data to the client.
	/// </summary>
	/// <param name="message">The message to send to the client.</param>
	void QueueSend( NetworkMessage message );
}
