using Serilog.Events;

namespace NetBolt.Server;

/// <summary>
/// Bootstraps the server.
/// </summary>
public static class Program
{
	/// <summary>
	/// The game to run.
	/// </summary>
	private static NetBoltGame _game = null!;

	/// <summary>
	/// The entry point to the program.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	public static void Main( string[] args )
	{
		var options = new GameOptions()
			.WithTickRate( 60 )
			.WithWelcomeMessage( "Welcome to the server!" )
			.WithLogLevel( LogEventLevel.Information );

		_game = new NetBoltGame( options );
		_game.Start();
	}
}
