using NetBolt.WebSocket.Options;

namespace NetBolt.Server;

/// <summary>
/// 
/// </summary>
public sealed class GameOptions : IReadOnlyGameOptions
{
	/// <summary>
	/// 
	/// </summary>
	public static GameOptions Default => new();
	
	/// <summary>
	/// 
	/// </summary>
	public int MaxEntities { get; set; } = 100_000;
	
	/// <summary>
	/// 
	/// </summary>
	public WebSocketServerOptions NetworkingOptions { get; set; } = new();
	/// <summary>
	/// 
	/// </summary>
	public IReadOnlyWebSocketServerOptions ReadOnlyNetworkingOptions => NetworkingOptions;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="maxEntities"></param>
	/// <returns></returns>
	public GameOptions WithMaxEntities( int maxEntities )
	{
		MaxEntities = maxEntities;
		return this;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	public GameOptions WithNetworkingOptions( WebSocketServerOptions options )
	{
		NetworkingOptions = options;
		return this;
	}
}
