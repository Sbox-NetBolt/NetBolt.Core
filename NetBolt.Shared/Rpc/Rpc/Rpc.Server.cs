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
	/// Executes an asynchronous RPC relating to a <see cref="ComplexNetworkable"/>s instance.
	/// </summary>
	/// <param name="client">The client to send the RPC to.</param>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> instance to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	[ServerOnly]
	public static async Task<RpcCallResponseMessage> CallAsync( INetworkClient client, ComplexNetworkable complexNetworkable, string methodName,
		params INetworkable[] parameters )
	{
		var message = CreateRpc( true, complexNetworkable, methodName, parameters );
		INetBoltServer.Instance.Send( client, message );
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
		INetBoltServer.Instance.Send( client, message );
		return await WaitForResponseAsync( message.CallGuid );
	}

	/// <summary>
	/// Executes an RPC relating to a <see cref="ComplexNetworkable"/>s instance that is sent to specific clients.
	/// </summary>
	/// <param name="to">The clients to send the RPC to.</param>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> instance to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	[ServerOnly]
	public static void Call( IEnumerable<INetworkClient> to, ComplexNetworkable complexNetworkable, string methodName, params INetworkable[] parameters )
	{
		INetBoltServer.Instance.Send( to, CreateRpc( false, complexNetworkable, methodName, parameters ) );
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
		INetBoltServer.Instance.Send( to, CreateRpc( false, type, methodName, parameters ) );
	}
}
