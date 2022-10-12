#if SERVER
using NetBolt.Server;
#endif
using NetBolt.Shared.Entities;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains an <see cref="IEntity"/> to delete.
/// </summary>
public sealed class DeleteEntityMessage : NetworkMessage
{
	/// <summary>
	/// The <see cref="IEntity"/> to delete.
	/// </summary>
	public IEntity Entity { get; private set; } = null!;

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="DeleteEntityMessage"/> with the <see cref="IEntity"/> that is being deleted.
	/// </summary>
	/// <param name="entity">The <see cref="IEntity"/> that is being deleted.</param>
	public DeleteEntityMessage( IEntity entity )
	{
		Entity = entity;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="DeleteEntityMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		var entityId = reader.ReadInt32();
		var entity = IEntity.All.GetEntityById( entityId );
		if ( entity is null )
		{
			Log.Error( $"Attempted to delete entity \"{entityId}\" which does not exist." );
			return;
		}

		Entity = entity;
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="DeleteEntityMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( Entity.EntityId );
	}
}
