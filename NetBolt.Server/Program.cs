namespace NetBolt.Server;

/// <summary>
/// Bootstraps the server.
/// </summary>
public static class Program
{
	/// <summary>
	/// The game to run.
	/// </summary>
	private static BaseGame _game = null!;

	/// <summary>
	/// The entry point to the program.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	public static void Main( string[] args )
	{
		_game = new BaseGame();
		_game.Start();
	}
}
