using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;
using System.Collections.Generic;

namespace NetBolt.Shared;

/// <summary>
/// The glue required for server-specific functionality.
/// </summary>
[ServerOnly]
public interface INetBoltServer
{
	/// <summary>
	/// Invoked when a <see cref="ComplexNetworkable"/> has been created.
	/// </summary>
	/// <param name="complexNetworkable">The base networkable that was created.</param>
	[ServerOnly]
	void OnBaseNetworkableCreated( ComplexNetworkable complexNetworkable );
	/// <summary>
	/// Invoked when a <see cref="ComplexNetworkable"/> has been deleted.
	/// </summary>
	/// <param name="complexNetworkable">The base networkable that was deleted.</param>
	[ServerOnly]
	void OnBaseNetworkableDeleted( ComplexNetworkable complexNetworkable );

	/// <summary>
	/// Sends a message to a client.
	/// </summary>
	/// <param name="client">The client to send the message to.</param>
	/// <param name="message">The message to send.</param>
	[ServerOnly]
	void Send( INetworkClient client, NetworkMessage message );
	/// <summary>
	/// Sends a message to a number of clients.
	/// </summary>
	/// <param name="clients">The clients to send the message to.</param>
	/// <param name="message">The message to send.</param>
	[ServerOnly]
	void Send( IEnumerable<INetworkClient> clients, NetworkMessage message );

	/// <summary>
	/// The instance to access for server glue functionality.
	/// </summary>
	public static INetBoltServer Instance { get; internal set; } = null!;
}
