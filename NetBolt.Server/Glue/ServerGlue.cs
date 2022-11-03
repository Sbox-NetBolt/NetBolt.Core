using NetBolt.Server.Utility;
using NetBolt.Shared;
using NetBolt.Shared.Clients;
using System.Collections.Generic;
using System.Linq;

namespace NetBolt.Server.Glue;

/// <summary>
/// The glue for running NetBolt on a server.
/// </summary>
public class ServerGlue : IGlue
{
	/// <inheritdoc/>
	public bool IsClient => false;
	/// <inheritdoc/>
	public bool IsServer => true;
	/// <inheritdoc/>
	public virtual string RealmName => "server";

	/// <inheritdoc/>
	public virtual ILogger Logger => _loggerGlue;
	/// <summary>
	/// The logger glue.
	/// </summary>
	private readonly LoggerGlue _loggerGlue = new();

	/// <inheritdoc/>
	public virtual ITypeLibrary TypeLibrary => _typeGlue;
	/// <summary>
	/// The type glue.
	/// </summary>
	private readonly TypeLibrary _typeGlue = new();

	/// <inheritdoc/>
	public virtual INetBoltServer Server => _serverSpecificGlue;
	/// <summary>
	/// The server specific glue.
	/// </summary>
	private readonly ServerSpecificGlue _serverSpecificGlue = new();

	/// <inheritdoc/>
	public INetBoltClient Client => null!;

	/// <inheritdoc/>
	public virtual IReadOnlyList<INetworkClient> Clients => GameServer.Instance.Clients;
	/// <inheritdoc/>
	public INetworkClient LocalClient => null!;

	/// <inheritdoc/>
	public virtual INetworkClient GetBot( long botId )
	{
		return GameServer.Instance.Bots.First( bot => bot.ClientId == botId );
	}

	/// <inheritdoc/>
	public virtual INetworkClient GetClient( long clientId )
	{
		return GameServer.Instance.Clients.First( client => client.ClientId == clientId );
	}
}
