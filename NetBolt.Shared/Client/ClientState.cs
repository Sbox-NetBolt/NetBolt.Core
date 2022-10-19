namespace NetBolt.Shared.Clients;

/// <summary>
/// Represents a state the client has changed to.
/// </summary>
public enum ClientState : byte
{
	/// <summary>
	/// Connected to the server.
	/// </summary>
	Connected,
	/// <summary>
	/// Disconnected from the server.
	/// </summary>
	Disconnected
}
