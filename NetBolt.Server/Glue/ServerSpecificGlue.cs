using NetBolt.Server.Utility;
using NetBolt.Shared;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using System.Collections.Generic;

namespace NetBolt.Server.Glue;

/// <summary>
/// The glue for server specific functionality.
/// </summary>
internal class ServerSpecificGlue : INetBoltServer
{
	/// <inheritdoc/>
	public void OnBaseNetworkableCreated( ComplexNetworkable complexNetworkable )
	{
		NetBoltGame.Current.OnBaseNetworkableCreated( complexNetworkable );
	}

	/// <inheritdoc/>
	public void OnBaseNetworkableDeleted( ComplexNetworkable complexNetworkable )
	{
		NetBoltGame.Current.OnBaseNetworkableDeleted( complexNetworkable );
	}

	/// <inheritdoc/>
	public void Send( INetworkClient client, NetworkMessage message )
	{
		GameServer.Instance.QueueSend( ToExtensions.Single( client ), message );
	}

	/// <inheritdoc/>
	public void Send( IEnumerable<INetworkClient> clients, NetworkMessage message )
	{
		GameServer.Instance.QueueSend( ToExtensions.Multiple( clients ), message );
	}
}
