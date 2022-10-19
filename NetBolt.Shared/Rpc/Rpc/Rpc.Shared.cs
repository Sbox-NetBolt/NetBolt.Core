using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.RemoteProcedureCalls;

/// <summary>
/// A collection of methods to execute Remote Procedure Calls (RPCs).
/// </summary>
public static partial class Rpc
{
	/// <summary>
	/// The dictionary to hold RPC responses.
	/// </summary>
	public static readonly Dictionary<Guid, RpcCallResponseMessage> Responses = new();

	/// <summary>
	/// Executes an RPC relating to a <see cref="BaseNetworkable"/> instance.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> instance to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	public static void Call( BaseNetworkable baseNetworkable, string methodName, params INetworkable[] parameters )
	{
		if ( Realm.IsClient )
			IGlue.Instance.Client.SendToServer( CreateRpc( false, baseNetworkable, methodName, parameters ) );
		else
			Call( INetworkClient.All, baseNetworkable, methodName, parameters );
	}

	/// <summary>
	/// Executes an RPC on a static method.
	/// </summary>
	/// <param name="type">The type to call the RPC on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	public static void Call( Type type, string methodName, params INetworkable[] parameters )
	{
		if ( Realm.IsClient )
			IGlue.Instance.Client.SendToServer( CreateRpc( false, type, methodName, parameters ) );
		else
			Call( INetworkClient.All, type, methodName, parameters );
	}

	/// <summary>
	/// Creates an RPC call message for an entity.
	/// </summary>
	/// <param name="respondable">Whether or not the RPC is expecting a response.</param>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> that is the target of the RPC.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>The built RPC message.</returns>
	private static RpcCallMessage CreateRpc( bool respondable, BaseNetworkable baseNetworkable, string methodName,
		INetworkable[] parameters )
	{
		return new RpcCallMessage( respondable, baseNetworkable.GetType(), baseNetworkable, methodName, parameters );
	}

	/// <summary>
	/// Creates an RPC call message for a static method.
	/// </summary>
	/// <param name="respondable">Whether or not the RPC is expecting a response.</param>
	/// <param name="type">The type that holds the method.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>The built RPC message.</returns>
	private static RpcCallMessage CreateRpc( bool respondable, Type type, string methodName, INetworkable[] parameters )
	{
		return new RpcCallMessage( respondable, type, null, methodName, parameters );
	}

	/// <summary>
	/// Awaits for a response on the specific RPC with the provided <see cref="Guid"/>.
	/// </summary>
	/// <param name="callGuid">The <see cref="Guid"/> to wait for a response on.</param>
	/// <returns>The response for the call.</returns>
	private static async Task<RpcCallResponseMessage> WaitForResponseAsync( Guid callGuid )
	{
		// TODO: This does not account for disconnects or the environment shutting down.
		while ( !Responses.ContainsKey( callGuid ) )
			await Task.Delay( 1 );

		Responses.Remove( callGuid, out var response );
		if ( response is null )
		{
			IGlue.Instance.Logger.Error( "Failed to return RPC response (\"{0}\" became invalid unexpectedly).", callGuid );
			return default!;
		}

		return response;
	}

	/// <summary>
	/// Marks a method to be a client-side RPC.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class ClientAttribute : Attribute
	{
		/// <summary>
		/// The amount of times the server can execute the RPC per second.
		/// </summary>
		public double LimitPerSecond;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientAttribute"/> with a limit on its executions per second.
		/// </summary>
		/// <param name="limitPerSecond">The maximum amount of times the RPC can be invoked per second.</param>
		public ClientAttribute( double limitPerSecond = double.MaxValue )
		{
			LimitPerSecond = limitPerSecond;
		}
	}

	/// <summary>
	/// Marks a method to be a server-side RPC.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class ServerAttribute : Attribute
	{
		/// <summary>
		/// The amount of times a client can execute the RPC per second.
		/// </summary>
		public double LimitPerSecond;

		/// <summary>
		/// Initializes a new instance of the <see cref="ServerAttribute"/> with a limit on its executions per second.
		/// </summary>
		/// <param name="limitPerSecond">The maximum amount of times the RPC can be invoked per second.</param>
		public ServerAttribute( double limitPerSecond = double.MaxValue )
		{
			LimitPerSecond = limitPerSecond;
		}
	}
}
