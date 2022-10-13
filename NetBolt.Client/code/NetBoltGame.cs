using System.Diagnostics;
using NetBolt.Client.UI;
using NetBolt.Shared;
using Sandbox;

namespace NetBolt.Client;

/// <summary>
/// The base game for NetBolt networking games.
/// </summary>
public class NetBoltGame : Game
{
	/// <summary>
	/// The only instance of <see cref="NetBoltGame"/> existing.
	/// </summary>
	public new static NetBoltGame Current => (Sandbox.Game.Current as NetBoltGame)!;

	/// <summary>
	/// An instance of <see cref="NetworkManager"/>.
	/// </summary>
	private readonly NetworkManager? _networkManager;
	/// <summary>
	/// An instance of <see cref="GameHud"/>.
	/// </summary>
	private readonly GameHud? _gameHud;

	/// <summary>
	/// The maximum tick rate to update networking at.
	/// </summary>
	protected virtual int TickRate => 15;
	/// <summary>
	/// The target delta time for the server.
	/// </summary>
	protected float TickRateDt => (float)1000 / TickRate;

	/// <summary>
	/// The stopwatch to track when to do a network update.
	/// </summary>
	private readonly Stopwatch _tickStopwatch = Stopwatch.StartNew();

	/// <summary>
	/// Initializes a new instance of <see cref="NetBoltGame"/>.
	/// </summary>
	public NetBoltGame()
	{
		if ( !IsClient )
			return;

		_networkManager = new NetworkManager();
		NetworkManager.ConnectedToServer += OnConnectedToServer;
		NetworkManager.DisconnectedFromServer += OnDisconnectedFromServer;
		NetworkManager.ClientConnected += OnClientConnected;
		NetworkManager.ClientDisconnected += OnClientDisconnected;
		_gameHud = new GameHud();

	}

	/// <summary>
	/// Called for every tick.
	/// </summary>
	[Event.Tick.Client]
	protected virtual void Update()
	{
		if ( _networkManager is null || !_networkManager.Connected )
			return;

		if ( _tickStopwatch.Elapsed.TotalMilliseconds < TickRateDt )
			return;

		_networkManager.Update();
		_tickStopwatch.Restart();
	}

	/// <summary>
	/// Invoked when connected to a server.
	/// </summary>
	protected virtual void OnConnectedToServer()
	{
		Log.Info( "Connected" );
		_tickStopwatch.Restart();
	}

	/// <summary>
	/// Invoked when disconnected from a server.
	/// </summary>
	protected virtual void OnDisconnectedFromServer()
	{
		Log.Info( "Disconnected" );
	}

	/// <summary>
	/// Invoked when a client joins the server.
	/// </summary>
	/// <param name="client">The client that has joined.</param>
	protected virtual void OnClientConnected( INetworkClient client )
	{
		ClientChatBox.AddInformation( $"{client.ClientId} has joined", $"avatar:{client.ClientId}" );
	}

	/// <summary>
	/// Invoked when a client has disconnected.
	/// </summary>
	/// <param name="client">The client that has disconnected.</param>
	protected virtual void OnClientDisconnected( INetworkClient client )
	{
		ClientChatBox.AddInformation( $"{client.ClientId} has left", $"avatar{client.ClientId}" );
	}

	/// <summary>
	/// Client console command to connect to a server.
	/// </summary>
	/// <param name="ip">The IP of the server to join.</param>
	/// <param name="port">The port of the server to join.</param>
	[ConCmd.Client( "connect_to_server" )]
	public static async void ConnectToServer( string ip, int port )
	{
		if ( NetworkManager.Instance is null )
			return;

		await NetworkManager.Instance.ConnectAsync( ip, port, false );
	}

	/// <summary>
	/// Client console command to disconnect from a server.
	/// </summary>
	[ConCmd.Client( "disconnect_from_server" )]
	public static async void DisconnectFromServer()
	{
		if ( NetworkManager.Instance is null )
			return;

		await NetworkManager.Instance.CloseAsync();
	}
}
