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
		_game = new NetBoltGame( GameOptions.Default );
		_game.Start();
	}
}
