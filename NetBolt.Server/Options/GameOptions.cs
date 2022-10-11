using NetBolt.WebSocket.Options;

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
	/// The maximum amount of entities that can exist in the game.
	/// </summary>
	public int MaxEntities { get; set; } = 100_000;

	/// <summary>
	/// The networking options to be applied to the game server.
	/// </summary>
	public WebSocketServerOptions NetworkingOptions { get; set; } = new();
	/// <summary>
	/// A read-only version of <see cref="NetworkingOptions"/>.
	/// </summary>
	public IReadOnlyWebSocketServerOptions ReadOnlyNetworkingOptions => NetworkingOptions;

	/// <summary>
	/// Sets the <see cref="MaxEntities"/> option.
	/// </summary>
	/// <param name="maxEntities">The maximum amount of entities that can exist in the game.</param>
	/// <returns>The config instance.</returns>
	public GameOptions WithMaxEntities( int maxEntities )
	{
		MaxEntities = maxEntities;
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
