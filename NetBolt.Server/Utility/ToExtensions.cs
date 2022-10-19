using NetBolt.Shared.Clients;
using NetBolt.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace NetBolt.Server.Utility;

/// <summary>
/// Extension class for <see cref="To"/>.
/// </summary>
public static class ToExtensions
{
	/// <summary>
	/// Targets a single <see cref="INetworkClient"/>.
	/// </summary>
	/// <param name="client">The client to target.</param>
	/// <returns>The target.</returns>
	public static To Single( INetworkClient client )
	{
		return To.Single( GetClient( client ) );
	}

	/// <summary>
	/// Targets multiple <see cref="INetworkClient"/>s.
	/// </summary>
	/// <param name="clients">The clients to target.</param>
	/// <returns>The target.</returns>
	public static To Multiple( IEnumerable<INetworkClient> clients )
	{
		return To.Multiple( GetClients( clients ) );
	}

	/// <summary>
	/// Targets all clients except for the provided clients.
	/// </summary>
	/// <param name="clientsToIgnore">The clients to ignore.</param>
	/// <returns>The target.</returns>
	public static To AllExcept( IEnumerable<INetworkClient> clientsToIgnore )
	{
		return To.AllExcept( GameServer.Instance, GetClients( clientsToIgnore ) );
	}

	/// <summary>
	/// Targets al clients except for the provided clients.
	/// </summary>
	/// <param name="clientsToIgnore">The clients to ignore.</param>
	/// <returns>The target.</returns>
	public static To AllExcept( params INetworkClient[] clientsToIgnore )
	{
		return To.AllExcept( GameServer.Instance, GetClients( clientsToIgnore ) );
	}

	/// <summary>
	/// Gets a <see cref="IClient"/> from a <see cref="INetworkClient"/>.
	/// </summary>
	/// <param name="client">The client to get.</param>
	/// <returns>The client that was found.</returns>
	private static IClient GetClient( INetworkClient client )
	{
		return GameServer.Instance.Clients.First( cl => cl.ClientId == client.ClientId );
	}

	/// <summary>
	/// Gets multiple <see cref="IClient"/>s from a number of <see cref="INetworkClient"/>s.
	/// </summary>
	/// <param name="clients">The clients to get.</param>
	/// <returns>The clients that were found.</returns>
	private static IEnumerable<IClient> GetClients( IEnumerable<INetworkClient> clients )
	{
		foreach ( var client in clients )
			yield return GetClient( client );
	}
}
