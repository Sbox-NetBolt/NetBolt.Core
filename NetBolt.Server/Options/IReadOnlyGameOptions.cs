using NetBolt.WebSocket.Options;
using Serilog.Events;

namespace NetBolt.Server;

/// <summary>
/// A read-only instance of <see cref="GameOptions"/>.
/// </summary>
public interface IReadOnlyGameOptions
{
	/// <summary>
	/// See <see cref="GameOptions.TickRate"/>.
	/// </summary>
	int TickRate { get; }

	/// <summary>
	/// See <see cref="GameOptions.WelcomeMessage"/>.
	/// </summary>
	string WelcomeMessage { get; }

	/// <summary>
	/// See <see cref="GameOptions.LogLevel"/>
	/// </summary>
	LogEventLevel LogLevel { get; }

	/// <summary>
	/// See <see cref="GameOptions.NetworkingOptions"/>.
	/// </summary>
	IReadOnlyWebSocketServerOptions ReadOnlyNetworkingOptions { get; }
}
