using NetBolt.Shared.Clients;
using NetBolt.Shared.Utility;
using System.Collections.Generic;

namespace NetBolt.Shared;

/// <summary>
/// The glue required to have NetBolt running in a realm.
/// </summary>
public interface IGlue
{
	/// <summary>
	/// Whether or not the realm we are in is considered the client.
	/// </summary>
	bool IsClient { get; }
	/// <summary>
	/// Whether or not the realm we are in is considered the server.
	/// </summary>
	bool IsServer { get; }
	/// <summary>
	/// The name of the realm we are executing in.
	/// </summary>
	string RealmName { get; }

	/// <summary>
	/// Glue to handle logging messages.
	/// </summary>
	ILogger Logger { get; }
	/// <summary>
	/// Glue to handle reflection actions.
	/// </summary>
	ITypeLibrary TypeLibrary { get; }

	/// <summary>
	/// Glue to handle server-specific actions.
	/// </summary>
	[ServerOnly]
	INetBoltServer Server { get; }
	/// <summary>
	/// Glue to handle client-specific actions.
	/// </summary>
	[ClientOnly]
	INetBoltClient Client { get; }

	/// <summary>
	/// A read-only list of all clients in the server.
	/// </summary>
	IReadOnlyList<INetworkClient> Clients { get; }
	/// <summary>
	/// The <see cref="INetworkClient"/> of the local client.
	/// </summary>
	[ClientOnly]
	INetworkClient LocalClient { get; }

	/// <summary>
	/// Gets a bot by its unique identifier.
	/// <remarks>This could also create a new bot and return that.</remarks>
	/// </summary>
	/// <param name="botId">The unique identifier of the bot.</param>
	/// <returns>The bot associated with the unique identifier.</returns>
	INetworkClient GetBot( long botId );
	/// <summary>
	/// Gets a client by its unique identifier.
	/// <remarks>This could also create a new client and return that.</remarks>
	/// </summary>
	/// <param name="clientId">The unique identifier of the client.</param>
	/// <returns>The client associated with the unique identifier.</returns>
	INetworkClient GetClient( long clientId );

	/// <summary>
	/// The instance to access for glue functionality.
	/// </summary>
	public static IGlue Instance { get; set; } = null!;
}
