using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.RemoteProcedureCalls;

public partial class Rpc
{
	/// <summary>
	/// Executes an asynchronous RPC relating to a <see cref="BaseNetworkable"/>s instance.
	/// </summary>
	/// <param name="client">The client to send the RPC to.</param>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> instance to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	[ServerOnly]
	public static async Task<RpcCallResponseMessage> CallAsync( INetworkClient client, BaseNetworkable baseNetworkable, string methodName,
		params INetworkable[] parameters )
	{
		var message = CreateRpc( true, baseNetworkable, methodName, parameters );
		IGlue.Instance.Server.Send( client, message );
		return await WaitForResponseAsync( message.CallGuid );
	}

	/// <summary>
	/// Executes an asynchronous RPC on a static method.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="type">The type to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	[ServerOnly]
	public static async Task<RpcCallResponseMessage> CallAsync( INetworkClient client, Type type, string methodName,
		params INetworkable[] parameters )
	{
		var message = CreateRpc( true, type, methodName, parameters );
		IGlue.Instance.Server.Send( client, message );
		return await WaitForResponseAsync( message.CallGuid );
	}

	/// <summary>
	/// Executes an RPC relating to a <see cref="BaseNetworkable"/>s instance that is sent to specific clients.
	/// </summary>
	/// <param name="to">The clients to send the RPC to.</param>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> instance to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	[ServerOnly]
	public static void Call( IEnumerable<INetworkClient> to, BaseNetworkable baseNetworkable, string methodName, params INetworkable[] parameters )
	{
		IGlue.Instance.Server.Send( to, CreateRpc( false, baseNetworkable, methodName, parameters ) );
	}

	/// <summary>
	/// Executes an RPC on a static method that is sent to specific clients.
	/// </summary>
	/// <param name="to">The clients to send the RPC to.</param>
	/// <param name="type">The type to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	[ServerOnly]
	public static void Call( IEnumerable<INetworkClient> to, Type type, string methodName, params INetworkable[] parameters )
	{
		IGlue.Instance.Server.Send( to, CreateRpc( false, type, methodName, parameters ) );
	}
}
