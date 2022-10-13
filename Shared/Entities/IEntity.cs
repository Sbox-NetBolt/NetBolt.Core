#if CLIENT
using NetBolt.Client;
#endif
using System.Collections.Generic;
using System.Linq;
using NetBolt.Shared.Networkables;

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
	/// Gets an entity by its unique network identifier.
	/// </summary>
	/// <param name="networkId">The unique network identifier of the entity.</param>
	/// <returns>An <see cref="IEntity"/> if found. Null if not</returns>
	public static IEntity? GetEntityById( int networkId )
	{
		return All.FirstOrDefault( entity => entity.EntityId == networkId );
	}
}
