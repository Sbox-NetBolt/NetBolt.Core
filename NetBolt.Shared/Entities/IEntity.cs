using System;
using System.Collections.Generic;
using System.Linq;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Entities;

/// <summary>
/// Contract to define something that is an entity in the game world.
/// </summary>
public interface IEntity : INetworkable
{
	/// <summary>
	/// The unique identifier of the <see cref="IEntity"/>.
	/// </summary>
	int EntityId { get; }

	/// <summary>
	/// The <see cref="INetworkClient"/> that owns this <see cref="IEntity"/>.
	/// </summary>
	INetworkClient? Owner { get; set; }

	/// <summary>
	/// Logic update for this <see cref="IEntity"/>.
	/// <remarks>This will be called every server tick or client frame.</remarks>
	/// </summary>
	void Update();

	/// <summary>
	/// A read-only list of all entities in the server.
	/// </summary>
	public static IReadOnlyList<IEntity> All => AllEntities;
	/// <summary>
	/// A list of all entities in the server.
	/// </summary>
	internal static List<IEntity> AllEntities { get; } = new();

	/// <summary>
	/// Gets an <see cref="IEntity"/> by its unique network identifier.
	/// </summary>
	/// <param name="networkId">The unique network identifier of the <see cref="IEntity"/>.</param>
	/// <returns>The <see cref="IEntity"/> that was found. Null if <see ref="networkId"/> is -1.</returns>
	public static IEntity? GetById( int networkId )
	{
		return networkId == -1 ? null : All.First( entity => entity.EntityId == networkId );
	}

	/// <summary>
	/// Attempts to get a <see cref="IEntity"/> by its unique network identifier. If no <see cref="IEntity"/> is found, then a request will be made with the provided callback.
	/// </summary>
	/// <param name="networkId">The unique network identifier of the <see cref="IEntity"/>.</param>
	/// <param name="cb">The callback to invoke when the <see cref="IEntity"/> exists.</param>
	/// <returns>The <see cref="IEntity"/> if found. Null otherwise.</returns>
	[ClientOnly]
	public static IEntity? GetOrRequestById( int networkId, Action<IEntity> cb )
	{
		if ( networkId == -1 )
			return null;

		var entity = All.FirstOrDefault( entity => entity.NetworkId == networkId );
		if ( entity is not null )
			return entity;

		INetBoltClient.Instance.RequestEntity( networkId, cb );
		return null;
	}
}
