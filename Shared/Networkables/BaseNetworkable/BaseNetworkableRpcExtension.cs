using System.Threading.Tasks;
#if SERVER
#endif
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
#if SERVER
using NetBolt.WebSocket;
#endif

namespace NetBolt.Shared.RemoteProcedureCalls;

/// <summary>
/// Extension of helper <see cref="BaseNetworkable"/> methods to call RPCs on themselves.
/// </summary>
public static class BaseNetworkableRpcExtension
{
	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.Call( BaseNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	public static void CallRpc( this BaseNetworkable baseNetworkable, string methodName, params INetworkable[] parameters )
	{
		Rpc.Call( baseNetworkable, methodName, parameters );
	}

#if CLIENT
	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.CallAsync( BaseNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	public static async Task<RpcCallResponseMessage> CallRpcAsync( this BaseNetworkable baseNetworkable, string methodName,
		params INetworkable[] parameters )
	{
		return await Rpc.CallAsync( baseNetworkable, methodName, parameters );
	}
#endif

#if SERVER
	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.Call( To, BaseNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> to call the RPC on.</param>
	/// <param name="to">The clients to execute the RPC.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	public static void CallRpc( this BaseNetworkable baseNetworkable, To to, string methodName, params INetworkable[] parameters )
	{
		Rpc.Call( to, baseNetworkable, methodName, parameters );
	}

	/// <summary>
	/// Wrapper for <see cref="Rpc"/>.<see cref="Rpc.CallAsync( INetworkClient, BaseNetworkable, string, INetworkable[] )"/>.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> to call the RPC on.</param>
	/// <param name="client">The client to execute the RPC.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>A task that will complete once a <see cref="RpcCallResponseMessage"/> is received that contains the sent <see cref="RpcCallMessage"/>.<see cref="RpcCallMessage.CallGuid"/>.</returns>
	public static async Task<RpcCallResponseMessage> CallRpcAsync( this BaseNetworkable baseNetworkable, INetworkClient client,
		string methodName, params INetworkable[] parameters )
	{
		return await Rpc.CallAsync( client, baseNetworkable, methodName, parameters );
	}
#endif
}
