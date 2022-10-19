using NetBolt.Shared.Exceptions;

namespace NetBolt.Shared.Utility;

/// <summary>
/// A utility class for realm specific checks.
/// </summary>
public static class Realm
{
	/// <summary>
	/// Whether or not the realm we are in is considered the client.
	/// </summary>
	public static bool IsClient => IGlue.Instance.IsClient;
	/// <summary>
	/// Whether or not the realm we are in is considered the server.
	/// </summary>
	public static bool IsServer => IGlue.Instance.IsServer;
	/// <summary>
	/// The name of the realm we are executing in.
	/// </summary>
	public static string Name => IGlue.Instance.RealmName;

	/// <summary>
	/// Asserts that the executing code is in the client realm.
	/// </summary>
	/// <exception cref="RealmException">Thrown if the executing code is not in the client realm.</exception>
	public static void AssertClient()
	{
		if ( !IsClient )
			throw new RealmException( false );
	}

	/// <summary>
	/// Asserts that the executing code is in the server realm.
	/// </summary>
	/// <exception cref="RealmException">Thrown if the executing code is not in the server realm.</exception>
	public static void AssertServer()
	{
		if ( !IsServer )
			throw new RealmException( true );
	}
}
