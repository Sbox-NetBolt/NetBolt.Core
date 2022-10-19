namespace NetBolt.Shared.Exceptions;

/// <summary>
/// Represents an error specific to incorrect realms.
/// </summary>
public class RealmException : NetBoltException
{
	/// <summary>
	/// Initializes a new instance of <see cref="RealmException"/>.
	/// </summary>
	/// <param name="expectedServer">Whether or not we expected to be in the server realm.</param>
	public RealmException( bool expectedServer )
		: base( $"Expected to be in {(expectedServer ? "server" : "client")}, but we are in {(!expectedServer ? "server" : "client")}" )
	{
	}
}
