using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.RemoteProcedureCalls;

public partial class Rpc
{
	/// <summary>
	/// Executes an asynchronous RPC relating to a <see cref="ComplexNetworkable"/> instance.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> instance to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	[ClientOnly]
	public static async Task<RpcCallResponseMessage> CallAsync( ComplexNetworkable complexNetworkable, string methodName,
		params INetworkable[] parameters )
	{
		var message = CreateRpc( true, complexNetworkable, methodName, parameters );
		INetBoltClient.Instance.SendToServer( message );
		return await WaitForResponseAsync( message.CallGuid );
	}

	/// <summary>
	/// Executes an asynchronous RPC on a static method.
	/// </summary>
	/// <param name="type">The type to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	[ClientOnly]
	public static async Task<RpcCallResponseMessage> CallAsync( Type type, string methodName,
		params INetworkable[] parameters )
	{
		var message = CreateRpc( true, type, methodName, parameters );
		INetBoltClient.Instance.SendToServer( message );
		return await WaitForResponseAsync( message.CallGuid );
	}
}
