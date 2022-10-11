#if SERVER
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetBolt.Server;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Utility;
using NetBolt.WebSocket;
using NetBolt.WebSocket.Exceptions;

namespace NetBolt.Shared;

public partial class NetworkClient
{
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkClient"/> with the socket it connected to and the server that is controlling the socket.
	/// </summary>
	/// <param name="socket">The socket the client is using.</param>
	/// <param name="server">The server the socket is a part of.</param>
	internal NetworkClient( TcpClient socket, IWebSocketServer server ) : base( socket, server )
	{
	}
	
	/// <summary>
	/// Serializes a message and sends the data to the client.
	/// </summary>
	/// <param name="message">The message to send to the client.</param>
	public void QueueSend( NetworkMessage message )
	{
		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );
		writer.WriteNetworkable( message );
		writer.Close();

		QueueSend( stream.ToArray() );
	}

	/// <summary>
	/// Invoked when a Binary message has been received.
	/// </summary>
	/// <param name="bytes">he data that was sent by the client.</param>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	protected override void OnData( ReadOnlySpan<byte> bytes )
	{
		base.OnData( bytes );

		var reader = new NetworkReader( new MemoryStream( bytes.ToArray() ) );
		var message = NetworkMessage.DeserializeMessage( reader );
		reader.Close();

		GameServer.Instance.QueueIncoming( this, message );
	}

	/// <summary>
	/// Invoked when the client is being verified for their handshake.
	/// </summary>
	/// <param name="headers">The handshake headers sent by the client.</param>
	/// <param name="request">The full request the client sent.</param>
	/// <returns>The async task that spawns from the invoke. The return value of the task is the whether or not to accept the clients handshake.</returns>
	/// <exception cref="ClientDisconnectedException">Thrown when using this method while the client is disconnected.</exception>
	protected override ValueTask<bool> VerifyHandshake( IReadOnlyDictionary<string, string> headers, string request )
	{
		if ( !headers.TryGetValue( "Steam", out var clientIdStr ) )
			return new ValueTask<bool>( false );

		if ( !long.TryParse( clientIdStr, out var clientId ) )
			return new ValueTask<bool>( false );

		if ( GameServer.Instance.GetClientById( clientId ) is not null )
			return new ValueTask<bool>( false );

		ClientId = clientId;
		return base.VerifyHandshake( headers, request );
	}
}
#endif
