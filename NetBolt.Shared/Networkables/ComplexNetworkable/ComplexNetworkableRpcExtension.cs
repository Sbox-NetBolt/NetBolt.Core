using System.Collections.Generic;
using System.Threading.Tasks;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.RemoteProcedureCalls;

/// <summary>
/// Extension of helper <see cref="ComplexNetworkable"/> methods to call RPCs on themselves.
/// </summary>
public static class ComplexNetworkableRpcExtension
{
	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.Call( ComplexNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	public static void CallRpc( this ComplexNetworkable complexNetworkable, string methodName, params INetworkable[] parameters )
	{
		Rpc.Call( complexNetworkable, methodName, parameters );
	}

	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.CallAsync( ComplexNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	[ClientOnly]
	public static async Task<RpcCallResponseMessage> CallRpcAsync( this ComplexNetworkable complexNetworkable, string methodName,
		params INetworkable[] parameters )
	{
		return await Rpc.CallAsync( complexNetworkable, methodName, parameters );
	}

	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.Call( IEnumerable{INetworkClient}, ComplexNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> to call the RPC on.</param>
	/// <param name="to">The clients to execute the RPC.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	[ServerOnly]
	public static void CallRpc( this ComplexNetworkable complexNetworkable, IEnumerable<INetworkClient> to, string methodName, params INetworkable[] parameters )
	{
		Rpc.Call( to, complexNetworkable, methodName, parameters );
	}

	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.CallAsync( INetworkClient, ComplexNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> to call the RPC on.</param>
	/// <param name="client">The client to execute the RPC.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	[ServerOnly]
	public static async Task<RpcCallResponseMessage> CallRpcAsync( this ComplexNetworkable complexNetworkable, INetworkClient client,
		string methodName, params INetworkable[] parameters )
	{
		return await Rpc.CallAsync( client, complexNetworkable, methodName, parameters );
	}
}
