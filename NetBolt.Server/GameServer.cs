using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using NetBolt.Shared;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Utility;
using NetBolt.WebSocket;
using NetBolt.WebSocket.Enums;
using NetBolt.WebSocket.Options;

namespace NetBolt.Server;

/// <summary>
/// The game server instance.
/// </summary>
internal sealed class GameServer : WebSocketServer
{
	/// <summary>
	/// The game server instance.
	/// </summary>
	internal static GameServer Instance { get; set; } = null!;

	/// <summary>
	/// 
	/// </summary>
	internal new IReadOnlyList<INetworkClient> Clients => _networkClients;
	/// <summary>
	/// 
	/// </summary>
	private readonly List<INetworkClient> _networkClients = new();
	/// <summary>
	/// 
	/// </summary>
	internal IReadOnlyList<BotClient> Bots => _botClients;
	/// <summary>
	/// 
	/// </summary>
	private readonly List<BotClient> _botClients = new();

	/// <summary>
	/// The handlers for incoming messages.
	/// </summary>
	private readonly Dictionary<Type, Action<INetworkClient, NetworkMessage>> _messageHandlers = new();
	/// <summary>
	/// The queue for messages incoming to the server.
	/// </summary>
	private readonly ConcurrentQueue<(INetworkClient, NetworkMessage)> _incomingQueue = new();

	internal GameServer( IReadOnlyWebSocketServerOptions options ) : base( options )
	{
	}

	public INetworkClient? GetClientById( long clientId )
	{
		return Clients.FirstOrDefault( client => client.ClientId == clientId );
	}

	/// <summary>
	/// Queues a message to be processed by the server.
	/// </summary>
	/// <remarks>This should only be used in cases where a <see cref="BotClient"/> is doing something.</remarks>
	/// <param name="client">The client that sent the message.</param>
	/// <param name="message">The message the client has sent.</param>
	public void QueueIncoming( INetworkClient client, NetworkMessage message )
	{
		_incomingQueue.Enqueue( (client, message) );
	}

	/// <summary>
	/// Queues a message to be sent to clients.
	/// </summary>
	/// <param name="to">The client(s) to send the message to.</param>
	/// <param name="message">The message to send to each client.</param>
	public void QueueSend( To to, NetworkMessage message )
	{
		// Quick send message to bots.
		foreach ( var client in to )
		{
			if ( client is BotClient bot )
				bot.QueueSend( message );
		}

		// Write message once.
		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );
		writer.WriteNetworkable( message );
		writer.Close();

		var bytes = stream.ToArray();
		foreach ( var client in to )
		{
			if ( client is BotClient )
				continue;

			client.QueueSend( bytes );
		}
	}

	public override void AcceptClient( IWebSocketClient client )
	{
		if ( client is not NetworkClient networkClient )
			throw new Exception( "Cannot accept non network clients to the server" );

		base.AcceptClient( client );

		_networkClients.Add( networkClient );
		if ( client is BotClient bot )
			_botClients.Add( bot );
	}

	protected override IWebSocketClient CreateClient( TcpClient client )
	{
		return new NetworkClient( client, this );
	}

	public override void OnClientUpgraded( IWebSocketClient client )
	{
		base.OnClientUpgraded( client );

		BaseGame.Current.OnClientConnected( (client as INetworkClient)! );
	}

	public override void OnClientDisconnected( IWebSocketClient client, WebSocketDisconnectReason reason, WebSocketError? error )
	{
		base.OnClientDisconnected( client, reason, error );

		BaseGame.Current.OnClientDisconnected( (client as INetworkClient)! );
	}

	/// <summary>
	/// Dispatches any incoming server messages.
	/// </summary>
	internal void DispatchIncoming()
	{
		while ( _incomingQueue.TryDequeue( out var pair ) )
		{
			if ( !_messageHandlers.TryGetValue( pair.Item2.GetType(), out var cb ) )
			{
				Logging.Error( $"Unhandled message type {pair.Item2.GetType()}." );
				continue;
			}

			cb.Invoke( pair.Item1, pair.Item2 );
		}
	}

	/// <summary>
	/// Adds a handler for the server to dispatch the message to.
	/// </summary>
	/// <param name="cb">The method to call when a message of type <see ref="T"/> has come in.</param>
	/// <typeparam name="T">The message type to handle.</typeparam>
	/// <exception cref="Exception">Thrown when a handler has already been set for <see ref="T"/>.</exception>
	internal void HandleMessage<T>( Action<INetworkClient, NetworkMessage> cb ) where T : NetworkMessage
	{
		var messageType = typeof( T );
		if ( _messageHandlers.ContainsKey( messageType ) )
		{
			Logging.Error( $"Message type {messageType} is already being handled." );
			return;
		}

		_messageHandlers.Add( messageType, cb );
	}
}
