using NetBolt.WebSocket.Options;

namespace NetBolt.Server;

public sealed class GameOptions
{
	public static GameOptions Default => new();
	
	public int MaxEntities { get; set; } = 100_000;
	public WebSocketServerOptions NetworkingOptions { get; set; } = new();

	public GameOptions WithMaxEntities( int maxEntities )
	{
		MaxEntities = maxEntities;
		return this;
	}

	public GameOptions WithNetworkingOptions( WebSocketServerOptions options )
	{
		NetworkingOptions = options;
		return this;
	}
}
