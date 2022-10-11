using NetBolt.WebSocket.Options;

namespace NetBolt.Server;

/// <summary>
/// 
/// </summary>
public interface IReadOnlyGameOptions
{
	/// <summary>
	/// 
	/// </summary>
	int MaxEntities { get; }
	
	/// <summary>
	/// 
	/// </summary>
	IReadOnlyWebSocketServerOptions ReadOnlyNetworkingOptions { get; }
}
