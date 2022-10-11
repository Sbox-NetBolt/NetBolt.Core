using NetBolt.WebSocket.Options;

namespace NetBolt.Server;

/// <summary>
/// A read-only instance of <see cref="GameOptions"/>.
/// </summary>
public interface IReadOnlyGameOptions
{
	/// <summary>
	/// See <see cref="GameOptions.MaxEntities"/>.
	/// </summary>
	int MaxEntities { get; }
	
	/// <summary>
	/// See <see cref="GameOptions.NetworkingOptions"/>.
	/// </summary>
	IReadOnlyWebSocketServerOptions ReadOnlyNetworkingOptions { get; }
}
