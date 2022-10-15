using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NetBolt.Shared;
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
	/// Whether or not the game is currently running.
	/// </summary>
	protected bool Running { get; private set; }

	/// <summary>
	/// The games cancellation source. If you want to exit the game then cancel this and the game will exit at the end of the tick.
	/// </summary>
	protected static readonly CancellationTokenSource ProgramCancellation = new();

	/// <summary>
	/// The network server handling communication of the game.
	/// </summary>
	private static GameServer _server = null!;

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
	/// </summary>
	public virtual void Start()
	{
		Log.Verbose( "Starting game..." );
		Running = true;

		GameServer.Instance.HandleMessage<RpcCallMessage>( Rpc.HandleRpcCallMessage );
		GameServer.Instance.HandleMessage<RpcCallResponseMessage>( Rpc.HandleRpcCallResponseMessage );
		GameServer.Instance.HandleMessage<ClientPawnUpdateMessage>( HandleClientPawnUpdateMessage );
		GameServer.Instance.HandleMessage<ClientSayMessage>( HandleClientSayMessage );

		_server.Start();
		Log.Info( "Server started on {A}:{B}", Options.ReadOnlyNetworkingOptions.IpAddress, Options.ReadOnlyNetworkingOptions.Port );

		var tickRateDt = (float)1000 / Options.TickRate;
		var currentTick = 0;
		var sw = Stopwatch.StartNew();
		while ( !ProgramCancellation.IsCancellationRequested )
		{
			// TODO: Cooking the CPU is not a very cool way of doing this
			while ( sw.Elapsed.TotalMilliseconds < tickRateDt )
			{
			}

			Time.Delta = (float)sw.Elapsed.TotalMilliseconds;
			Time.Tick = ++currentTick;
			sw.Restart();

			_server.DispatchIncoming();
			Update();

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
	public virtual void Update()
	{
		foreach ( var entity in IEntity.All )
			entity.Update();

		var changedBaseNetworkables = BaseNetworkable.All.Where( baseNetworkable => baseNetworkable.Changed() ).ToList();
		if ( changedBaseNetworkables.Count == 0 )
			return;

		Log.Verbose( $"{nameof( BaseNetworkable )}s changed, sending update..." );
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new MultiBaseNetworkableUpdateMessage( changedBaseNetworkables ) );
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/> has been authorized and has joined the server.
	/// </summary>
	/// <param name="client">The handle of the client that has connected.</param>
	public virtual void OnClientConnected( INetworkClient client )
	{
		Log.Info( "{A} has connected", client );

		var toClient = To.Single( client );
		GameServer.Instance.QueueSend( toClient, new WelcomeMessage( Options.TickRate, Options.WelcomeMessage ) );
		GameServer.Instance.QueueSend( toClient, new ClientListMessage( GameServer.Instance.Clients ) );
		GameServer.Instance.QueueSend( toClient, new BaseNetworkableListMessage( BaseNetworkable.All ) );
		GameServer.Instance.QueueSend( To.AllExcept( GameServer.Instance, client ), new ClientStateChangedMessage( client, ClientState.Connected ) );

		client.PawnChanged += ClientOnPawnChanged;
		client.Pawn = new BasePlayer();
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

		GameServer.Instance.QueueSend( To.AllExcept( GameServer.Instance, client ), new ClientStateChangedMessage( client, ClientState.Disconnected ) );
		(client.Pawn as BaseNetworkable)?.Delete();
		client.PawnChanged -= ClientOnPawnChanged;
	}

	/// <summary>
	/// Called when a <see cref="BaseNetworkable"/> is created.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> that has been created.</param>
	public virtual void OnBaseNetworkableCreated( BaseNetworkable baseNetworkable )
	{
		Log.Verbose( "{A} created", baseNetworkable );
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new CreateBaseNetworkableMessage( baseNetworkable ) );
	}

	/// <summary>
	/// Called when a <see cref="BaseNetworkable"/> is deleted.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> that has been deleted.</param>
	public virtual void OnBaseNetworkableDeleted( BaseNetworkable baseNetworkable )
	{
		Log.Verbose( "{A} deleted", baseNetworkable );
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new DeleteBaseNetworkableMessage( baseNetworkable ) );
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
		GameServer.Instance.QueueSend( To.AllExcept( GameServer.Instance, client ), clientSayMessage );
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
