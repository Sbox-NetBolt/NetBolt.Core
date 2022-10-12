using NetBolt.Client.UI;
using NetBolt.Shared;
using Sandbox;

namespace NetBolt.Client;

public class BaseGame : Game
{
	public new static BaseGame Current => (Sandbox.Game.Current as BaseGame)!;

	public readonly NetworkManager? NetworkManager;
	public readonly GameHud? GameHud;

	public BaseGame()
	{
		if ( !IsClient )
			return;

		NetworkManager = new NetworkManager();
		NetworkManager.ConnectedToServer += OnConnectedToServer;
		NetworkManager.DisconnectedFromServer += OnDisconnectedFromServer;
		NetworkManager.ClientConnected += OnClientConnected;
		NetworkManager.ClientDisconnected += OnClientDisconnected;
		GameHud = new GameHud();
	}

	[Event.Tick.Client]
	protected virtual void Update()
	{
		if ( NetworkManager is null || !NetworkManager.Connected )
			return;

		NetworkManager.DispatchIncoming();
		NetworkManager.Update();
		NetworkManager.DispatchOutgoing();
	}

	protected virtual void OnConnectedToServer()
	{
		Log.Info( "Connected" );
	}

	protected virtual void OnDisconnectedFromServer()
	{
		Log.Info( "Disconnected" );
	}

	protected virtual void OnClientConnected( INetworkClient client )
	{
		ClientChatBox.AddInformation( $"{client.ClientId} has joined", $"avatar:{client.ClientId}" );
	}

	protected virtual void OnClientDisconnected( INetworkClient client )
	{
		ClientChatBox.AddInformation( $"{client.ClientId} has left", $"avatar{client.ClientId}" );
	}

	[ConCmd.Client( "connect_to_server" )]
	public static void ConnectToServer( string uri, int port )
	{
		_ = NetworkManager.Instance?.ConnectAsync( uri, port, false );
	}

	[ConCmd.Client( "disconnect_from_server" )]
	public static void DisconnectFromServer()
	{
		NetworkManager.Instance?.Close();
	}
}
