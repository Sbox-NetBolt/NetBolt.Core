using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NetBolt.Shared;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.RemoteProcedureCalls;
using NetBolt.Shared.Utility;
using NetBolt.WebSocket;
using NetBolt.WebSocket.Options;

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
	
	public GameOptions Options { get; }

	/// <summary>
	/// Manages all server-side only entities.
	/// </summary>
	internal readonly EntityManager ServerEntityManager = new();
	/// <summary>
	/// Manages all networked entities.
	/// </summary>
	internal readonly EntityManager SharedEntityManager = new();

	/// <summary>
	/// The maximum tick rate of the server. In the event of severe performance hits the tick rate can drop below this desired number.
	/// </summary>
	protected virtual int TickRate => 60;
	/// <summary>
	/// The target delta time for the server.
	/// </summary>
	protected float TickRateDt => (float)1000 / TickRate;

	/// <summary>
	/// The whole programs cancellation source. If you want to exit the program then cancel this and the program will exit at the end of the tick.
	/// </summary>
	private static readonly CancellationTokenSource ProgramCancellation = new();
	/// <summary>
	/// The network server handling communication of the game.
	/// </summary>
	private static GameServer _server = null!;

	public NetBoltGame( GameOptions options )
	{
		if ( Current is not null )
			Log.Fatal( new InvalidOperationException( $"An instance of {nameof( NetBoltGame )} already exists." ) );

		Current = this;
		Options = options;

		Log.Initialize();
		Log.Info( "Log started" );

		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		_server = new GameServer( options.NetworkingOptions );
		GameServer.Instance = _server;
		_server.Start();
	}

	/// <summary>
	/// Called at the start of the program. Use this to do a one-time startup of all needed things in your game.
	/// <remarks>At this point the networking server has not started.</remarks>
	/// </summary>
	public virtual void Start()
	{
		GameServer.Instance.HandleMessage<RpcCallMessage>( Rpc.HandleRpcCallMessage );
		GameServer.Instance.HandleMessage<RpcCallResponseMessage>( Rpc.HandleRpcCallResponseMessage );
		GameServer.Instance.HandleMessage<ClientPawnUpdateMessage>( HandleClientPawnUpdateMessage );

		SharedEntityManager.EntityCreated += OnNetworkedEntityCreated;
		SharedEntityManager.EntityDeleted += OnNetworkedEntityDeleted;

		var sw = Stopwatch.StartNew();
		while ( !ProgramCancellation.IsCancellationRequested )
		{
			// TODO: Cooking the CPU is not a very cool way of doing this
			while ( sw.Elapsed.TotalMilliseconds < TickRateDt )
			{
			}

			Time.Delta = (float)sw.Elapsed.TotalMilliseconds;
			sw.Restart();

			_server.DispatchIncoming();
			Update();
		}
	}

	/// <summary>
	/// Called at the end of the program. Use this to cleanup any resources you have collected over the duration of running.
	/// <remarks>At this point the networking server is still running but do not expect it to send any messages at this point.</remarks>
	/// </summary>
	public virtual void Shutdown()
	{
		ServerEntityManager.DeleteAllEntities();
		SharedEntityManager.DeleteAllEntities();
	}

	/// <summary>
	/// Called at every tick of the program. Use this for your core game logic.
	/// <remarks>If overriding, it is highly recommended to call the base class method after your code. Otherwise, networked entities you have edited won't be sent to clients till the next tick.</remarks>
	/// </summary>
	public virtual void Update()
	{
		foreach ( var serverEntity in ServerEntityManager.Entities.Values )
			serverEntity.Update();

		foreach ( var sharedEntity in SharedEntityManager.Entities.Values )
			sharedEntity.Update();

		// TODO: PVS type system?
		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );
		var countPos = writer.BaseStream.Position;
		writer.BaseStream.Position += sizeof( int );

		var count = 0;
		foreach ( var entity in SharedEntityManager.Entities.Values )
		{
			if ( !entity.Changed() )
				continue;

			count++;
			writer.Write( entity.EntityId );
			entity.SerializeChanges( writer );
		}

		var tempPos = writer.BaseStream.Position;
		writer.BaseStream.Position = countPos;
		writer.Write( count );
		writer.BaseStream.Position = tempPos;
		writer.Close();

		if ( count != 0 )
			GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new MultiEntityUpdateMessage( stream.ToArray() ) );
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/> has been authorized and has joined the server.
	/// </summary>
	/// <param name="client">The handle of the client that has connected.</param>
	public virtual void OnClientConnected( INetworkClient client )
	{
		Log.Info( $"{client} has connected" );

		var toClient = To.Single( client );
		GameServer.Instance.QueueSend( toClient, new ClientListMessage( GameServer.Instance.Clients ) );
		GameServer.Instance.QueueSend( toClient, new EntityListMessage( SharedEntityManager.Entities.Values ) );
		GameServer.Instance.QueueSend( To.AllExcept( GameServer.Instance, client ), new ClientStateChangedMessage( client.ClientId, ClientState.Connected ) );

		client.PawnChanged += ClientOnPawnChanged;
		client.Pawn = SharedEntityManager.Create<BasePlayer>();
		client.Pawn.Owner = client;
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/> has disconnected from the server. This could be intentional or due to a timeout.
	/// </summary>
	/// <param name="client">The handle of the client that has disconnected.</param>
	public virtual void OnClientDisconnected( INetworkClient client )
	{
		Log.Info( $"{client} has disconnected" );

		GameServer.Instance.QueueSend( To.AllExcept( GameServer.Instance, client ), new ClientStateChangedMessage( client.ClientId, ClientState.Disconnected ) );
		if ( client.Pawn is not null )
			SharedEntityManager.DeleteEntity( client.Pawn );
		client.PawnChanged -= ClientOnPawnChanged;
	}

	/// <summary>
	/// Gets an <see cref="IEntity"/> that is local to the server.
	/// </summary>
	/// <param name="entityId">The ID of the <see cref="IEntity"/> to get.</param>
	/// <returns>The <see cref="IEntity"/> that was found. Null if no <see cref="IEntity"/> was found.</returns>
	public IEntity? GetLocalEntityById( int entityId )
	{
		return ServerEntityManager.GetEntityById( entityId );
	}

	/// <summary>
	/// Gets an <see cref="IEntity"/> that is available to both client and server.
	/// </summary>
	/// <param name="entityId">The ID of the <see cref="IEntity"/> to get.</param>
	/// <returns>The <see cref="IEntity"/> that was found. Null if no <see cref="IEntity"/> was found.</returns>
	public IEntity? GetNetworkedEntityById( int entityId )
	{
		return SharedEntityManager.GetEntityById( entityId );
	}

	/// <summary>
	/// Called when an <see cref="IEntity"/> is created in the <see cref="SharedEntityManager"/>.
	/// </summary>
	/// <param name="entity">The <see cref="IEntity"/> that has been created.</param>
	protected virtual void OnNetworkedEntityCreated( IEntity entity )
	{
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new CreateEntityMessage( entity ) );
	}

	/// <summary>
	/// Called when an <see cref="IEntity"/> is deleted in the <see cref="SharedEntityManager"/>.
	/// </summary>
	/// <param name="entity">The <see cref="IEntity"/> that has been deleted.</param>
	protected virtual void OnNetworkedEntityDeleted( IEntity entity )
	{
		GameServer.Instance.QueueSend( To.All( GameServer.Instance ), new DeleteEntityMessage( entity ) );
	}

	/// <summary>
	/// Called when a <see cref="INetworkClient"/>s pawn has been swapped.
	/// </summary>
	/// <param name="client">The <see cref="INetworkClient"/> that has its pawn changed.</param>
	/// <param name="oldpawn">The old <see cref="IEntity"/> the <see ref="client"/> was controlling.</param>
	/// <param name="newPawn">The new <see cref="IEntity"/> the <see ref="client"/> is now controlling.</param>
	protected virtual void ClientOnPawnChanged( INetworkClient client, IEntity? oldpawn, IEntity? newPawn )
	{
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
	/// Handler for when the program is shutting down.
	/// </summary>
	private void OnProcessExit( object? sender, EventArgs e )
	{
		Log.Info( "Shutting down..." );
		Shutdown();
		ProgramCancellation.Cancel();
		_server.StopAsync().Wait();

		Log.Info( "Log finished" );
		Log.Dispose();
	}

	private void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
	{
		Log.Fatal( (Exception)e.ExceptionObject );
		OnProcessExit( null, EventArgs.Empty );
	}
}