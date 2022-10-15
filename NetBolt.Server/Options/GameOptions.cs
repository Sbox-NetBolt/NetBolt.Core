using NetBolt.WebSocket.Options;
using Serilog.Events;

namespace NetBolt.Server;

/// <summary>
/// A configuration class for a <see cref="NetBoltGame"/>.
/// </summary>
public sealed class GameOptions : IReadOnlyGameOptions
{
	/// <summary>
	/// Creates a default instance of <see cref="GameOptions"/>.
	/// </summary>
	public static GameOptions Default => new();

	/// <summary>
	/// The maximum tick rate of the server.
	/// <remarks>In the event of severe performance hits the tick rate can drop below this desired number.</remarks>
	/// </summary>
	public int TickRate { get; set; } = 60;

	/// <summary>
	/// A default welcome message to send to a client when they join the server.
	/// </summary>
	public string WelcomeMessage { get; set; } = "Welcome!";

	/// <summary>
	/// The maximum level at which things will be logged.
	/// </summary>
	public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

	/// <summary>
	/// The networking options to be applied to the game server.
	/// </summary>
	public WebSocketServerOptions NetworkingOptions { get; set; } = new();
	/// <summary>
	/// A read-only version of <see cref="NetworkingOptions"/>.
	/// </summary>
	public IReadOnlyWebSocketServerOptions ReadOnlyNetworkingOptions => NetworkingOptions;

	/// <summary>
	/// Sets the <see cref="TickRate"/> option.
	/// </summary>
	/// <param name="tickRate">The maximum tick rate of the server.</param>
	/// <returns>The config instance.</returns>
	public GameOptions WithTickRate( int tickRate )
	{
		TickRate = tickRate;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="WelcomeMessage"/> option.
	/// </summary>
	/// <param name="welcomeMessage">A default welcome message to send to a client when they join the server.</param>
	/// <returns>The config instance.</returns>
	public GameOptions WithWelcomeMessage( string welcomeMessage )
	{
		WelcomeMessage = welcomeMessage;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="LogLevel"/> option.
	/// </summary>
	/// <param name="logLevel">The maximum level at which things will be logged.</param>
	/// <returns>The config instance.</returns>
	public GameOptions WithLogLevel( LogEventLevel logLevel )
	{
		LogLevel = logLevel;
		return this;
	}

	/// <summary>
	/// Sets the <see cref="NetworkingOptions"/> option.
	/// </summary>
	/// <param name="options">The networking options to be applied to the game server.</param>
	/// <returns>The config instance.</returns>
	public GameOptions WithNetworkingOptions( WebSocketServerOptions options )
	{
		NetworkingOptions = options;
		return this;
	}
}
