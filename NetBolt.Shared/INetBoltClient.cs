using NetBolt.Shared.Messages;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared;

/// <summary>
/// The glue required for client-specific functionality.
/// </summary>
[ClientOnly]
public interface INetBoltClient
{
	/// <summary>
	/// Sends a message to the server.
	/// </summary>
	/// <param name="message">The message to send.</param>
	[ClientOnly]
	void SendToServer( NetworkMessage message );
}
