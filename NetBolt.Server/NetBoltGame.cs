using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NetBolt.Server.Glue;
using NetBolt.Server.Utility;
using NetBolt.Shared;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.RemoteProcedureCalls;
using NetBolt.Shared.Utility;
using NetBolt.WebSocket;
using NetBolt.WebSocket.Enums;

namespace NetBolt.Server;

/// <summary>
/// The base class for any game servers.
/// </summary>
public class NetBoltGame
{
	/// <summary>
	/// The only instance of the game in existence.
	/// </summary>
	public static NetBoltGame Current = null!;

	/// <summary>
	/// A read-only instance of game options this game was created with.
	/// </summary>
	public IReadOnlyGameOptions Options { get; }

	/// <summary>
	/// The current tick of the server.
	/// </summary>
	protected int CurrentTick { get; set; }

	/// <summary>
	/// Whether or not the game is currently running.
	/// </summary>
	protected bool Running { get; private set; }

	/// <summary>
	/// The games cancellation source. If you want to exit the game then cancel this and the game will exit at the end of the tick.
	/// </summary>
	protected readonly CancellationTokenSource ProgramCancellation = new();

	/// <summary>
	/// The network server handling communication of the game.
	/// </summary>
	private GameServer _server = null!;

	/// <summary>
	/// Initializes a new instance of <see cref="NetBoltGame"/> with the provided configuration.
	/// </summary>
	/// <param name="options">The configuration of the game.</param>
	public NetBoltGame( IReadOnlyGameOptions options )
	{
		if ( Current is not null )
			Log.Fatal( new InvalidOperationException( $"An instance of {nameof( NetBoltGame )} already exists." ) );

		Current = this;
		Options = options;
		IGlue.Instance = new ServerGlue();

		Log.Initialize();
		Log.Info( "Log started" );

		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		_server = new GameServer( options.ReadOnlyNetworkingOptions );
		GameServer.Instance = _server;
	}

	/// <summary>
	/// Called at the start of the program. Use this to do a one-time startup of all needed things in your game.
	/// <remarks>At this point the networking server has not started.</remarks>
	/// <param name="execute">Whether or not the server should run the update logic itself.</param>
	/// </summary>
	public virtual void Start( bool execute = true )
	{
		Log.Verbose( "Starting game..." );
		Running = true;

		GameServer.Instance.HandleMessage<RpcCallMessage>( HandleRpcCallMessage );
		GameServer.Instance.HandleMessage<RpcCallResponseMessage>( HandleRpcCallResponseMessage );
		GameServer.Instance.HandleMessage<ClientPawnUpdateMessage>( HandleClientPawnUpdateMessage );
		GameServer.Instance.HandleMessage<ClientSayMessage>( HandleClientSayMessage );

		_server.Start();
		Log.Info( "Server started on {A}:{B}", Options.ReadOnlyNetworkingOptions.IpAddress, Options.ReadOnlyNetworkingOptions.Port );

		if ( !execute )
			return;

		var tickRateDt = (float)1000 / Options.TickRate;
		var sw = Stopwatch.StartNew();
		while ( !ProgramCancellation.IsCancellationRequested )
		{
			// TODO: Cooking the CPU is not a very cool way of doing this
			while ( sw.Elapsed.TotalMilliseconds < tickRateDt )
			{
			}

			var dt = (float)sw.Elapsed.TotalMilliseconds;
			sw.Restart();

			Tick( dt );

			Log.Debug( "Tick took {A}ms", sw.Elapsed.TotalMilliseconds );
		}
	}

	/// <summary>
	/// Called at the end of the program. Use this to cleanup any resources you have collected over the duration of running.
	/// <remarks>At this point the networking server is still running but do not expect it to send any messages at this point.</remarks>
	/// </summary>
	protected virtual void Shutdown()
	{
		Log.Info( "Shutting down game..." );
		Running = false;
		ProgramCancellation.Cancel();

		if ( _server.Running )
			_server.StopAsync().Wait();
	}

	/// <summary>
	/// Called at every tick of the program. Use this for your core game logic.
	/// <remarks>If overriding, it is highly recommended to call the base class method after your code. Otherwise, networked entities you have edited won't be sent to clients till the next tick.</remarks>
	/// </summary>
	public virtual void Tick( float dt )
	{
		Time.Delta = dt;
		Time.Tick = ++CurrentTick;

		_server.DispatchIncoming();

		foreach ( var entity in IEntity.All )
			entity.Update();

		var changedComplexNetworkables = ComplexNetworkable.All.Where( complexNetworkable => complexNetworkable.Changed() ).ToList();
		if ( changedComplexNetworkables.Count == 0 )
			return;

		Log.Verbose( $"{nameof( ComplexNetworkable )}s changed, sending update..." );
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new MultiComplexNetworkableUpdateMessage( changedComplexNetworkables ) );
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/> has been authorized and has joined the server.
	/// </summary>
	/// <param name="client">The handle of the client that has connected.</param>
	public virtual void OnClientConnected( INetworkClient client )
	{
		Log.Info( "{A} has connected", client );

		var toClient = ToExtensions.Single( client );
		GameServer.Instance.QueueSend( toClient, new WelcomeMessage( Options.TickRate, Options.WelcomeMessage, TypeLibrary.Instance.GetNetworkableDictionary() ) );
		GameServer.Instance.QueueSend( toClient, new ClientListMessage( GameServer.Instance.Clients ) );
		GameServer.Instance.QueueSend( toClient, new ComplexNetworkableListMessage( ComplexNetworkable.All ) );
		GameServer.Instance.QueueSend( ToExtensions.AllExcept( client ), new ClientStateChangedMessage( client, ClientState.Connected ) );

		client.PawnChanged += ClientOnPawnChanged;
		var player = new BasePlayer();
		player.Components.AddOrGetComponent<ColorComponent>();
		client.Pawn = player;
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/> has disconnected from the server. This could be intentional or due to a timeout.
	/// </summary>
	/// <param name="client">The handle of the client that has disconnected.</param>
	/// <param name="reason">The reason for the client disconnecting.</param>
	/// <param name="error">The error associated with the disconnect.</param>
	public virtual void OnClientDisconnected( INetworkClient client, WebSocketDisconnectReason reason, WebSocketError? error )
	{
		if ( reason == WebSocketDisconnectReason.Error )
			Log.Info( "{A} has disconnected for reason: {B} ({C})", client, reason, error );
		else
			Log.Info( "{A} has disconnected for reason: {B}", client, reason );

		GameServer.Instance.QueueSend( ToExtensions.AllExcept( client ), new ClientStateChangedMessage( client, ClientState.Disconnected ) );
		(client.Pawn as ComplexNetworkable)?.Delete();
		client.PawnChanged -= ClientOnPawnChanged;
	}

	/// <summary>
	/// Called when a <see cref="ComplexNetworkable"/> is created.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> that has been created.</param>
	public virtual void OnComplexNetworkableCreated( ComplexNetworkable complexNetworkable )
	{
		Log.Verbose( "{A} created", complexNetworkable );
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new CreateComplexNetworkableMessage( complexNetworkable ) );
	}

	/// <summary>
	/// Called when a <see cref="ComplexNetworkable"/> is deleted.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> that has been deleted.</param>
	public virtual void OnComplexNetworkableDeleted( ComplexNetworkable complexNetworkable )
	{
		Log.Verbose( "{A} deleted", complexNetworkable );
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new DeleteComplexNetworkableMessage( complexNetworkable ) );
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/>s pawn has been swapped.
	/// </summary>
	/// <param name="client">The <see cref="INetworkClient"/> that has its pawn changed.</param>
	/// <param name="oldpawn">The old <see cref="IEntity"/> the <see ref="client"/> was controlling.</param>
	/// <param name="newPawn">The new <see cref="IEntity"/> the <see ref="client"/> is now controlling.</param>
	protected virtual void ClientOnPawnChanged( INetworkClient client, IEntity? oldpawn, IEntity? newPawn )
	{
		Log.Verbose( "{A}s pawn changed from {B} to {C}", client, oldpawn, newPawn );
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new ClientPawnChangedMessage( client, oldpawn, newPawn ) );
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/>s <see cref="INetworkClient.Pawn"/> has updated and the server needs to process it.
	/// </summary>
	/// <param name="client">The <see cref="INetworkClient"/> that sent this update.</param>
	/// <param name="message">The <see cref="NetworkMessage"/> of the update.</param>
	protected virtual void HandleClientPawnUpdateMessage( INetworkClient client, NetworkMessage message )
	{
		if ( message is not ClientPawnUpdateMessage clientPawnUpdateMessage )
			return;

		if ( client.Pawn is null )
		{
			Log.Error( $"Received a {nameof( ClientPawnUpdateMessage )} when the client has no pawn." );
			return;
		}

		var reader = new NetworkReader( new MemoryStream( clientPawnUpdateMessage.PartialPawnData ) );
		client.Pawn.DeserializeChanges( reader );
		reader.Close();
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/> has sent a chat message.
	/// </summary>
	/// <param name="client">The <see cref="INetworkClient"/> that sent this update.</param>
	/// <param name="message">The message the <see ref="client"/> sent.</param>
	protected virtual void HandleClientSayMessage( INetworkClient client, NetworkMessage message )
	{
		if ( message is not ClientSayMessage clientSayMessage )
			return;

		if ( clientSayMessage.Client is null )
		{
			Log.Error( $"Received {nameof( ClientSayMessage )} from unknown client" );
			return;
		}

		Log.Info( "{A}: {B}", clientSayMessage.Client, clientSayMessage.Message );
		GameServer.Instance.QueueSend( ToExtensions.AllExcept( client ), clientSayMessage );
	}

	/// <summary>
	/// Handles an incoming RPC from a client.
	/// </summary>
	/// <param name="client">The client that sent the RPC.</param>
	/// <param name="message">The RPC call message.</param>
	/// <exception cref="InvalidOperationException">Thrown when handling the RPC call failed.</exception>
	protected virtual void HandleRpcCallMessage( INetworkClient client, NetworkMessage message )
	{
		if ( message is not RpcCallMessage rpcCall )
			return;

		var type = TypeLibrary.Instance.GetTypeByName( rpcCall.ClassName );
		if ( type is null )
			throw new InvalidOperationException( $"Failed to handle RPC call (\"{rpcCall.ClassName}\" doesn't exist in any accessible assemblies)." );

		var method = type.GetMethod( rpcCall.MethodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
		if ( method is null )
			throw new InvalidOperationException( $"Failed to handle RPC call (\"{rpcCall.MethodName}\" does not exist on \"{type}\")." );

		if ( method.GetCustomAttribute<Rpc.ServerAttribute>() is null )
			throw new InvalidOperationException( "Failed to handle RPC call (Attempted to invoke a non-RPC method)." );

		var complexNetworkable = ComplexNetworkable.GetById( rpcCall.ComplexNetworkableId );
		if ( complexNetworkable is null && rpcCall.ComplexNetworkableId != -1 )
			throw new InvalidOperationException( $"Failed to handle RPC call (Attempted to call RPC on a non-existant {nameof( ComplexNetworkable )})." );

		var returnValue = method.Invoke( complexNetworkable, rpcCall.Parameters );
		if ( rpcCall.CallGuid == Guid.Empty )
			return;

		if ( returnValue is not INetworkable && returnValue is not null )
		{
			var failedMessage = new RpcCallResponseMessage( rpcCall.CallGuid, RpcCallState.Failed );
			GameServer.Instance.QueueSend( ToExtensions.Single( client ), failedMessage );
			throw new InvalidOperationException(
				$"Failed to handle RPC call (\"{rpcCall.MethodName}\" returned a non-networkable value)." );
		}

		var response = new RpcCallResponseMessage( rpcCall.CallGuid, RpcCallState.Completed,
			returnValue as INetworkable ?? null );
		GameServer.Instance.QueueSend( ToExtensions.Single( client ), response );
	}

	/// <summary>
	/// Handles an incoming RPC call response.
	/// </summary>
	/// <param name="client">The client that sent the response.</param>
	/// <param name="message">The RPC call response.</param>
	/// <exception cref="InvalidOperationException">Thrown when handling the RPC call response failed.</exception>
	protected virtual void HandleRpcCallResponseMessage( INetworkClient client, NetworkMessage message )
	{
		if ( message is not RpcCallResponseMessage rpcResponse )
			return;

		if ( !Rpc.Responses.TryAdd( rpcResponse.CallGuid, rpcResponse ) )
			throw new InvalidOperationException( $"Failed to handle RPC call response (Failed to add \"{rpcResponse.CallGuid}\" to response dictionary)." );
	}

	/// <summary>
	/// Handler for when the program is shutting down.
	/// </summary>
	private void OnProcessExit( object? sender, EventArgs e )
	{
		Log.Info( "Shutting down..." );
		Shutdown();

		Log.Info( "Log finished" );
		Log.Dispose();
	}

	/// <summary>
	/// Handler for when an unhandled exception has been thrown.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event arguments for the unhandled exception.</param>
	private void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
	{
		Log.Fatal( (Exception)e.ExceptionObject, false );
		OnProcessExit( null, EventArgs.Empty );
	}
}
