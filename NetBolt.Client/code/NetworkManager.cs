using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetBolt.Client.Utility;
using NetBolt.Shared;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.RemoteProcedureCalls;
using NetBolt.Shared.Utility;
using Sandbox;
using BaseNetworkable = NetBolt.Shared.Networkables.BaseNetworkable;

namespace NetBolt.Client;

/// <summary>
/// Handles connection and general networking to a connected server.
/// </summary>
public sealed class NetworkManager
{
	/// <summary>
	/// The only instance of <see cref="NetworkManager"/> existing.
	/// </summary>
	public static NetworkManager? Instance;

#if DEBUG
	/// <summary>
	/// A debug variable for how many messages have been received from the server.
	/// </summary>
	public int MessagesReceived;
	/// <summary>
	/// A debug variable for how many messages have been sent to the server.
	/// </summary>
	public int MessagesSent;

	/// <summary>
	/// A debug variable for all of the messages that have been received and how many there have been.
	/// </summary>
	public readonly Dictionary<Type, int> MessageTypesReceived = new();
#endif

	/// <summary>
	/// A read-only list of all clients that the client knows about.
	/// </summary>
	public IReadOnlyList<INetworkClient> Clients => _clients;
	/// <summary>
	/// A list of all clients that the client knows about.
	/// </summary>
	private readonly List<INetworkClient> _clients = new();

	/// <summary>
	/// The local client.
	/// </summary>
	public INetworkClient LocalClient => GetClientById( _localClientId )!;

	/// <summary>
	/// Whether or not the client is connected to a server.
	/// </summary>
	public bool Connected { get; private set; }

	/// <summary>
	/// The delegate for handling when the client has connected to a server.
	/// </summary>
	public delegate void ConnectedEventHandler();
	/// <summary>
	/// Invoked when the client has connected to a server.
	/// </summary>
	public static event ConnectedEventHandler? ConnectedToServer;

	/// <summary>
	/// The delegate for handling when the client has disconnected from a server.
	/// </summary>
	public delegate void DisconnectedEventHandler();
	/// <summary>
	/// Invoked when the client has disconnected from a server.
	/// </summary>
	public static event DisconnectedEventHandler? DisconnectedFromServer;

	/// <summary>
	/// The delegate for handling when a client has connected to the server.
	/// </summary>
	public delegate void ClientConnectedEventHandler( INetworkClient client );
	/// <summary>
	/// Invoked when a client has connected to the server.
	/// </summary>
	public static event ClientConnectedEventHandler? ClientConnected;

	/// <summary>
	/// The delegate for handling when a client has disconnected from the server.
	/// </summary>
	public delegate void ClientDisconnectedEventHandler( INetworkClient client );
	/// <summary>
	/// Invoked when a client has disconnected from the server.
	/// </summary>
	public static event ClientDisconnectedEventHandler? ClientDisconnected;

	/// <summary>
	/// The client socket used to connect to the server.
	/// </summary>
	private WebSocket _webSocket;
	/// <summary>
	/// The handlers for incoming messages.
	/// </summary>
	private readonly Dictionary<Type, Action<NetworkMessage>> _messageHandlers = new();
	/// <summary>
	/// A queue of all incoming messages from the server.
	/// </summary>
	private readonly Queue<byte[]> _incomingQueue = new();
	/// <summary>
	/// A queue of all outgoing messages to the server.
	/// </summary>
	private readonly Queue<NetworkMessage> _outgoingQueue = new();
	/// <summary>
	/// A timer for when to stagger updates of the local clients pawn.
	/// </summary>
	private readonly Stopwatch _pawnSw = Stopwatch.StartNew();
	/// <summary>
	/// The unique client identifier of the client.
	/// </summary>
	private long _localClientId;

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkManager"/>.
	/// </summary>
	public NetworkManager()
	{
		if ( Instance is not null )
			Log.Error( new InvalidOperationException( $"An instance of {nameof( NetworkManager )} already exists." ) );

		Instance = this;
		_webSocket = new WebSocket();
		_webSocket.OnDisconnected += WebSocketOnDisconnected;
		_webSocket.OnDataReceived += WebSocketOnDataReceived;
		_webSocket.OnMessageReceived += WebSocketOnMessageReceived;

		HandleMessage<RpcCallMessage>( Rpc.HandleRpcCallMessage );
		HandleMessage<RpcCallResponseMessage>( Rpc.HandleRpcCallResponseMessage );
		HandleMessage<MultiMessage>( HandleMultiMessage );
		HandleMessage<ShutdownMessage>( HandleShutdownMessage );
		HandleMessage<ClientListMessage>( HandleClientListMessage );
		HandleMessage<BaseNetworkableListMessage>( HandleBaseNetworkableListMessage );
		HandleMessage<CreateBaseNetworkableMessage>( HandleCreateBaseNetworkableMessage );
		HandleMessage<DeleteBaseNetworkableMessage>( HandleDeleteBaseNetworkableMessage );
		HandleMessage<ClientStateChangedMessage>( HandleClientStateChangedMessage );
		HandleMessage<ClientPawnChangedMessage>( HandleClientPawnChangedMessage );
		HandleMessage<MultiBaseNetworkableUpdateMessage>( HandleMultiBaseNetworkableUpdateMessage );
	}

	/// <summary>
	/// Disconnects and closes the connection to the server.
	/// </summary>
	public async Task CloseAsync()
	{
		await _webSocket.CloseAsync( "disconnect" );
		Close();
	}

	/// <summary>
	/// Connects to a server.
	/// </summary>
	/// <param name="ip">The IP of the server to connect to.</param>
	/// <param name="port">The port of the server to connect to.</param>
	/// <param name="secure">Whether or not to use the web socket secure (wss://) protocol.</param>
	public async Task ConnectAsync( string ip, int port, bool secure )
	{
		if ( Connected )
			await CloseAsync();

		try
		{
			var rand = new Random( Time.Tick );
			_localClientId = rand.NextInt64();

			var headers = new Dictionary<string, string> { { "Steam", _localClientId.ToString() } };
			var webSocketUri = (secure ? "wss://" : "ws://") + ip + ':' + port + '/';
			Log.Info( "Connecting..." );
			await _webSocket.Connect( webSocketUri, headers );

			_clients.Add( new NetworkClient( _localClientId ) );
			Connected = true;
			ConnectedToServer?.Invoke();
		}
		catch ( Exception e )
		{
			Log.Error( e );
			await CloseAsync();
		}
	}

	/// <summary>
	/// Gets a client by its identifier.
	/// </summary>
	/// <param name="clientId">The identifier of the client to find.</param>
	/// <returns>The client if found. Null if not found.</returns>
	public INetworkClient? GetClientById( long clientId )
	{
		return clientId == -1 ? null : Clients.FirstOrDefault( client => client.ClientId == clientId );
	}

	/// <summary>
	/// Adds a handler for the client to dispatch the message to.
	/// </summary>
	/// <param name="cb">The method to call when a message of type <see ref="T"/> has come in.</param>
	/// <typeparam name="T">The message type to handle.</typeparam>
	public void HandleMessage<T>( Action<NetworkMessage> cb ) where T : NetworkMessage
	{
		var messageType = typeof( T );
		if ( _messageHandlers.ContainsKey( messageType ) )
			Log.Warning( $"Server message type {messageType} is already being handled. Overriding..." );

		_messageHandlers.Add( messageType, cb );
	}

	/// <summary>
	/// Queues a <see cref="NetworkMessage"/> to be sent to the server.
	/// </summary>
	/// <param name="message">The message to send to the server.</param>
	public void SendToServer( NetworkMessage message )
	{
		_outgoingQueue.Enqueue( message );
	}

	/// <summary>
	/// Called for every tick of the server.
	/// </summary>
	public void Update()
	{
		DispatchIncoming();
		foreach ( var baseNetworkable in BaseNetworkable.All )
			baseNetworkable.ProcessPendingNetworkables();

		foreach ( var entity in IEntity.All )
			entity.Update();

		if ( LocalClient.Pawn is not INetworkable pawn || !pawn.Changed() || _pawnSw.Elapsed.TotalMilliseconds < 100 )
			return;

		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );

		writer.WriteNetworkableChanges( ref pawn );
		writer.Close();

		SendToServer( new ClientPawnUpdateMessage( stream.ToArray() ) );
		_pawnSw.Restart();
		DispatchOutgoing();
	}

	/// <summary>
	/// Dispatches all of the incoming messages from the server.
	/// </summary>
	internal void DispatchIncoming()
	{
		while ( _incomingQueue.TryDequeue( out var bytes ) )
		{
			var reader = new NetworkReader( new MemoryStream( bytes ) );
			var message = NetworkMessage.DeserializeMessage( reader );
			reader.Close();
			DispatchMessage( message );
		}
	}

	/// <summary>
	/// Dispatches all of the outgoing messages to the server.
	/// </summary>
	internal void DispatchOutgoing()
	{
		while ( _outgoingQueue.TryDequeue( out var message ) )
		{
#if DEBUG
			MessagesSent++;
#endif
			var stream = new MemoryStream();
			var writer = new NetworkWriter( stream );
			writer.WriteNetworkable( message );
			writer.Close();

			_ = _webSocket?.Send( stream.ToArray() );
		}
	}

	/// <summary>
	/// Closes the connection to the server.
	/// </summary>
	private void Close()
	{
		Connected = false;
		_webSocket = new WebSocket();
		_clients.Clear();
		var entities = IEntity.All.ToArray();
		foreach ( var entity in entities )
			(entity as BaseNetworkable)?.Delete();
		IEntity.AllEntities.Clear();

#if DEBUG
		MessagesReceived = 0;
		MessagesSent = 0;
		MessageTypesReceived.Clear();
#endif

		DisconnectedFromServer?.Invoke();
	}

	/// <summary>
	/// Dispatches a message to its handler.
	/// </summary>
	/// <param name="message">The message to dispatch.</param>
	private void DispatchMessage( NetworkMessage message )
	{
#if DEBUG
		var messageType = message.GetType();
		if ( !MessageTypesReceived.ContainsKey( messageType ) )
			MessageTypesReceived.Add( messageType, 0 );
		MessageTypesReceived[messageType]++;
#endif

		if ( !_messageHandlers.TryGetValue( message.GetType(), out var cb ) )
		{
			Log.Error( $"Unhandled message type {message.GetType()}." );
			return;
		}

		cb.Invoke( message );
	}

	/// <summary>
	/// Handles a <see cref="MultiMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="MultiMessage"/> that was received.</param>
	private void HandleMultiMessage( NetworkMessage message )
	{
		if ( message is not MultiMessage multiMessage )
			return;

		foreach ( var msg in multiMessage.Messages )
			DispatchMessage( msg );
	}

	/// <summary>
	/// Handles a <see cref="ShutdownMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="ShutdownMessage"/> that was received.</param>
	private void HandleShutdownMessage( NetworkMessage message )
	{
		if ( message is not ShutdownMessage )
			return;

		Close();
	}

	/// <summary>
	/// Handles a <see cref="ClientListMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="ClientListMessage"/> that was received.</param>
	private void HandleClientListMessage( NetworkMessage message )
	{
		if ( message is not ClientListMessage clientListMessage )
			return;

		foreach ( var client in clientListMessage.Clients )
		{
			if ( client.ClientId == _localClientId )
				continue;

			_clients.Add( client );
		}
	}

	/// <summary>
	/// Handles a <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="BaseNetworkableListMessage"/> that was received.</param>
	private void HandleBaseNetworkableListMessage( NetworkMessage message )
	{
		// All the needed logic is handled when the message is deserialized
	}

	/// <summary>
	/// Handles a <see cref="CreateBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="CreateBaseNetworkableMessage"/> that was received.</param>
	private void HandleCreateBaseNetworkableMessage( NetworkMessage message )
	{
		if ( message is not CreateBaseNetworkableMessage createBaseNetworkableMessage )
			return;

		_ = TypeHelper.Create<BaseNetworkable>( createBaseNetworkableMessage.BaseNetworkableClass, createBaseNetworkableMessage.NetworkId );
	}

	/// <summary>
	/// Handles a <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="DeleteBaseNetworkableMessage"/> that was received.</param>
	private void HandleDeleteBaseNetworkableMessage( NetworkMessage message )
	{
		if ( message is not DeleteBaseNetworkableMessage deleteBaseNetworkableMessage )
			return;

		var baseNetworkable = BaseNetworkable.All.FirstOrDefault( baseNetworkable =>
			baseNetworkable.NetworkId == deleteBaseNetworkableMessage.NetworkId );
		baseNetworkable?.Delete();
	}

	/// <summary>
	/// Handles a <see cref="ClientStateChangedMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="ClientStateChangedMessage"/> that was received.</param>
	private void HandleClientStateChangedMessage( NetworkMessage message )
	{
		if ( message is not ClientStateChangedMessage clientStateChangedMessage )
			return;

		switch ( clientStateChangedMessage.ClientState )
		{
			case ClientState.Connected:
				_clients.Add( clientStateChangedMessage.Client );
				ClientConnected?.Invoke( clientStateChangedMessage.Client );
				break;
			case ClientState.Disconnected:
				if ( !_clients.Contains( clientStateChangedMessage.Client ) )
					return;

				_clients.Remove( clientStateChangedMessage.Client );
				ClientDisconnected?.Invoke( clientStateChangedMessage.Client );
				break;
			default:
				Log.Error( $"Got unexpected client state: {clientStateChangedMessage.ClientState}" );
				break;
		}
	}

	/// <summary>
	/// Handles a <see cref="ClientPawnChangedMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="ClientPawnChangedMessage"/> that was received.</param>
	private void HandleClientPawnChangedMessage( NetworkMessage message )
	{
		if ( message is not ClientPawnChangedMessage clientPawnChangedMessage )
			return;

		clientPawnChangedMessage.Client.Pawn = clientPawnChangedMessage.NewPawn;
	}

	/// <summary>
	/// Handles a <see cref="MultiBaseNetworkableUpdateMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="MultiBaseNetworkableUpdateMessage"/> that was received.</param>
	private void HandleMultiBaseNetworkableUpdateMessage( NetworkMessage message )
	{
		if ( message is not MultiBaseNetworkableUpdateMessage multiBaseNetworkableUpdateMessage )
			return;

		var reader = new NetworkReader( new MemoryStream( multiBaseNetworkableUpdateMessage.PartialBaseNetworkableData ) );
		var baseNetworkableCount = reader.ReadInt32();
		for ( var i = 0; i < baseNetworkableCount; i++ )
		{
			var networkId = reader.ReadInt32();
			var baseNetworkable = BaseNetworkable.All.FirstOrDefault( baseNetworkable => baseNetworkable.NetworkId == networkId );
			if ( baseNetworkable is null )
			{
				Log.Error( $"Attempted to update a {nameof( BaseNetworkable )} that does not exist." );
				continue;
			}

			reader.ReadNetworkableChanges( baseNetworkable );
		}
		reader.Close();
	}

	/// <summary>
	/// Invoked when the web socket has disconnected from the server.
	/// </summary>
	/// <param name="status">The status code that was received.</param>
	/// <param name="reason">The string reason that was received.</param>
	private void WebSocketOnDisconnected( int status, string reason )
	{
		Close();
	}

	/// <summary>
	/// Invoked when data has been received from the server.
	/// </summary>
	/// <param name="data">The data that was received.</param>
	private void WebSocketOnDataReceived( Span<byte> data )
	{
#if DEBUG
		MessagesReceived++;
#endif

		_incomingQueue.Enqueue( data.ToArray() );
	}

	/// <summary>
	/// Invoked when text has been received from the server.
	/// </summary>
	/// <param name="message">The text that was received.</param>
	private void WebSocketOnMessageReceived( string message )
	{
	}
}
