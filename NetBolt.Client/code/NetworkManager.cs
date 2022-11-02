using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetBolt.Client.UI;
using NetBolt.Client.Utility;
using NetBolt.Shared;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.RemoteProcedureCalls;
using NetBolt.Shared.Utility;
using Sandbox;

namespace NetBolt.Client;

/// <summary>
/// Handles connection and general networking to a connected server.
/// </summary>
public sealed class NetworkManager
{
	/// <summary>
	/// The only instance of <see cref="NetworkManager"/> existing.
	/// </summary>
	public static NetworkManager Instance = null!;

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
	/// A debug variable for all of the messages that have been sent and how many there have been.
	/// </summary>
	public readonly Dictionary<Type, int> MessageTypesSent = new();

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
	/// The tick rate that the server is running at.
	/// </summary>
	public int TickRate { get; private set; }

	/// <summary>
	/// The address that the client is connected to.
	/// </summary>
	public string Address { get; private set; } = "";
	/// <summary>
	/// The port that the client is connected to on the <see cref="Address"/>.
	/// </summary>
	public int Port { get; private set; }

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

		HandleMessage<RpcCallMessage>( HandleRpcCallMessage );
		HandleMessage<RpcCallResponseMessage>( HandleRpcCallResponseMessage );
		HandleMessage<MultiMessage>( HandleMultiMessage );
		HandleMessage<ShutdownMessage>( HandleShutdownMessage );
		HandleMessage<WelcomeMessage>( HandleWelcomeMessage );
		HandleMessage<ClientListMessage>( HandleClientListMessage );
		HandleMessage<BaseNetworkableListMessage>( HandleBaseNetworkableListMessage );
		HandleMessage<CreateBaseNetworkableMessage>( HandleCreateBaseNetworkableMessage );
		HandleMessage<DeleteBaseNetworkableMessage>( HandleDeleteBaseNetworkableMessage );
		HandleMessage<ClientStateChangedMessage>( HandleClientStateChangedMessage );
		HandleMessage<ClientPawnChangedMessage>( HandleClientPawnChangedMessage );
		HandleMessage<ClientSayMessage>( HandleClientSayMessage );
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

			_clients.Add( new NetworkClient( _localClientId, false ) );
			Connected = true;
			Address = ip;
			Port = port;
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
	/// <returns>The client if found. Null if provided -1.</returns>
	public INetworkClient? GetClientById( long clientId )
	{
		return clientId == -1 ? null : Clients.First( client => client.ClientId == clientId );
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
#if DEBUG
		var messageType = message.GetType();
		if ( !MessageTypesSent.ContainsKey( messageType ) )
			MessageTypesSent.Add( messageType, 0 );
		MessageTypesSent[messageType]++;
#endif

		_outgoingQueue.Enqueue( message );
	}

	/// <summary>
	/// Called for every tick of the server.
	/// </summary>
	public void Update()
	{
		DispatchIncoming();

		if ( LocalClient.Pawn is not INetworkable pawn || !pawn.Changed() )
		{
			DispatchOutgoing();
			return;
		}

		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );

		writer.WriteChanges( ref pawn );
		writer.Close();

		SendToServer( new ClientPawnUpdateMessage( stream.ToArray() ) );
		DispatchOutgoing();
	}

	/// <summary>
	/// Dispatches all of the incoming messages from the server.
	/// </summary>
	private void DispatchIncoming()
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
	private void DispatchOutgoing()
	{
		while ( _outgoingQueue.TryDequeue( out var message ) )
		{
#if DEBUG
			MessagesSent++;
#endif
			var stream = new MemoryStream();
			var writer = new NetworkWriter( stream );
			writer.Write( message );
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
		TickRate = 0;
		Address = string.Empty;
		Port = 0;

		_webSocket = new WebSocket();
		_webSocket.OnDisconnected += WebSocketOnDisconnected;
		_webSocket.OnDataReceived += WebSocketOnDataReceived;
		_webSocket.OnMessageReceived += WebSocketOnMessageReceived;

		_clients.Clear();
		var entities = IEntity.All.ToArray();
		foreach ( var entity in entities )
			(entity as ComplexNetworkable)?.Delete();
		IEntity.AllEntities.Clear();

#if DEBUG
		MessagesReceived = 0;
		MessagesSent = 0;
		MessageTypesSent.Clear();
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
	/// Handles an incoming RPC from the server.
	/// </summary>
	/// <param name="message">The RPC call message.</param>
	/// <exception cref="InvalidOperationException">Thrown when handling the RPC call failed.</exception>
	[ClientOnly]
	internal void HandleRpcCallMessage( NetworkMessage message )
	{
		if ( message is not RpcCallMessage rpcCall )
			return;

		var type = TypeLibrary.GetDescription( rpcCall.ClassName );
		if ( type is null )
		{
			Log.Error( $"Failed to handle RPC call (\"{rpcCall.ClassName}\" doesn't exist)." );
			return;
		}

		// TODO: Support instance methods https://github.com/Facepunch/sbox-issues/issues/2079
		var method = TypeLibrary.FindStaticMethods( rpcCall.MethodName ).First();
		if ( method is null )
		{
			Log.Error( $"Failed to handle RPC call (\"{rpcCall.MethodName}\" does not exist on \"{type}\")." );
			return;
		}

		if ( method.GetCustomAttribute<Rpc.ClientAttribute>() is null )
		{
			Log.Error( "Failed to handle RPC call (Attempted to invoke a non-RPC method)." );
			return;
		}

		var complexNetworkable = ComplexNetworkable.GetById( rpcCall.BaseNetworkableId );
		if ( complexNetworkable is null && rpcCall.NetworkId != -1 )
		{
			Log.Error( $"Failed to handle RPC call (Attempted to call RPC on a non-existant {nameof( ComplexNetworkable )})." );
			return;
		}

		var parameters = new List<object>();
		parameters.AddRange( rpcCall.Parameters );
		if ( complexNetworkable is not null )
			parameters.Insert( 0, complexNetworkable );

		if ( rpcCall.CallGuid == Guid.Empty )
		{
			method.Invoke( null, parameters.ToArray() );
			return;
		}

		var returnValue = method.InvokeWithReturn<object?>( null, parameters.ToArray() );
		if ( returnValue is not INetworkable && returnValue is not null )
		{
			var failedMessage = new RpcCallResponseMessage( rpcCall.CallGuid, RpcCallState.Failed );
			SendToServer( failedMessage );
			Log.Error( $"Failed to handle RPC call (\"{rpcCall.MethodName}\" returned a non-networkable value)." );
			return;
		}

		var response = new RpcCallResponseMessage( rpcCall.CallGuid, RpcCallState.Completed, returnValue as INetworkable ?? null );
		SendToServer( response );
	}

	/// <summary>
	/// Handles an incoming RPC call response.
	/// </summary>
	/// <param name="message">The RPC call response.</param>
	/// <exception cref="InvalidOperationException">Thrown when handling the RPC call response failed.</exception>
	[ClientOnly]
	internal void HandleRpcCallResponseMessage( NetworkMessage message )
	{
		if ( message is not RpcCallResponseMessage rpcResponse )
			return;

		Rpc.Responses.Add( rpcResponse.CallGuid, rpcResponse );
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

	private void HandleWelcomeMessage( NetworkMessage message )
	{
		if ( message is not WelcomeMessage welcomeMessage )
			return;

		TickRate = welcomeMessage.TickRate;
		ClientChatBox.AddInformation( welcomeMessage.Message );
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
		if ( message is not BaseNetworkableListMessage baseNetworkableListMessage )
			return;

		var clientGlue = (SandboxGlue.ClientSpecificGlue)INetBoltClient.Instance;
		foreach ( var complexNetworkable in baseNetworkableListMessage.BaseNetworkables )
			clientGlue.BaseNetworkableAvailable( complexNetworkable );
	}

	/// <summary>
	/// Handles a <see cref="CreateComplexNetworkableMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="CreateComplexNetworkableMessage"/> that was received.</param>
	private void HandleCreateBaseNetworkableMessage( NetworkMessage message )
	{
		if ( message is not CreateComplexNetworkableMessage createComplexNetworkableMessage )
			return;

		((SandboxGlue.ClientSpecificGlue)INetBoltClient.Instance).BaseNetworkableAvailable( createComplexNetworkableMessage.ComplexNetworkable );
	}

	/// <summary>
	/// Handles a <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="message">The <see cref="DeleteBaseNetworkableMessage"/> that was received.</param>
	private void HandleDeleteBaseNetworkableMessage( NetworkMessage message )
	{
		if ( message is not DeleteBaseNetworkableMessage deleteBaseNetworkableMessage )
			return;

		var complexNetworkable = ComplexNetworkable.All.FirstOrDefault( complexNetworkable =>
			complexNetworkable.NetworkId == deleteBaseNetworkableMessage.NetworkId );
		complexNetworkable?.Delete();
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

	private void HandleClientSayMessage( NetworkMessage message )
	{
		if ( message is not ClientSayMessage clientSayMessage )
			return;

		if ( clientSayMessage.Client is null )
		{
			Log.Error( $"Received {nameof( ClientSayMessage )} from an unknown client" );
			return;
		}

		ClientChatBox.AddChatEntry( clientSayMessage.Client.ClientId.ToString(), clientSayMessage.Message, $"avatar:{clientSayMessage.Client.ClientId}" );
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
			var complexNetworkable = ComplexNetworkable.All.FirstOrDefault( complexNetworkable => complexNetworkable.NetworkId == networkId );
			if ( complexNetworkable is null )
			{
				Log.Error( $"Attempted to update a {nameof( ComplexNetworkable )} that does not exist." );
				continue;
			}

			reader.ReadNetworkableChanges( complexNetworkable );
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
