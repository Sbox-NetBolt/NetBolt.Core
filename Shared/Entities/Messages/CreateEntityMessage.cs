using NetBolt.Shared.Entities;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains information to create a new <see cref="IEntity"/>.
/// </summary>
public sealed class CreateEntityMessage : NetworkMessage
{
	/// <summary>
	/// The class name of the <see cref="IEntity"/>.
	/// </summary>
	public string EntityClass { get; private set; } = "";
	/// <summary>
	/// The unique identifier the <see cref="IEntity"/> has.
	/// </summary>
	public int EntityId { get; private set; }

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="CreateEntityMessage"/> with the <see cref="IEntity"/> that is being created.
	/// </summary>
	/// <param name="entity">The <see cref="IEntity"/> that is being created.</param>
	public CreateEntityMessage( IEntity entity )
	{
		EntityClass = entity.GetType().Name;
		EntityId = entity.EntityId;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="CreateEntityMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		EntityClass = reader.ReadString();
		EntityId = reader.ReadInt32();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="CreateEntityMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( EntityClass );
		writer.Write( EntityId );
	}
}
