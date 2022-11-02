using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;
using System;

namespace NetBolt.Shared;

/// <summary>
/// The glue required for client-specific functionality.
/// </summary>
[ClientOnly]
public interface INetBoltClient
{
	/// <summary>
	/// Makes a request to callback when a <see cref="ComplexNetworkable"/> with the provided <see ref="networkId"/> exists.
	/// </summary>
	/// <param name="networkId">The unique network identifier to look for.</param>
	/// <param name="cb">The callback to invoke when the <see cref="ComplexNetworkable"/> exists.</param>
	[ClientOnly]
	void RequestBaseNetworkable( int networkId, Action<ComplexNetworkable> cb );
	/// <summary>
	/// Makes a request to callback when a <see cref="IEntity"/> with the provided <see ref="networkId"/> exists.
	/// </summary>
	/// <param name="networkId">The unique network identifier to look for.</param>
	/// <param name="cb">The callback to invoke when the <see cref="IEntity"/> exists.</param>
	[ClientOnly]
	void RequestEntity( int networkId, Action<IEntity> cb );

	/// <summary>
	/// Sends a message to the server.
	/// </summary>
	/// <param name="message">The message to send.</param>
	[ClientOnly]
	void SendToServer( NetworkMessage message );

	/// <summary>
	/// The instance to access for client glue functionality.
	/// </summary>
	public static INetBoltClient Instance { get; internal set; } = null!;
}
