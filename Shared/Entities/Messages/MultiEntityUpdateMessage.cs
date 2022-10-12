using System;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing information about an <see cref="IEntity"/> that has updated.
/// </summary>
public sealed class MultiEntityUpdateMessage : NetworkMessage
{
	/// <summary>
	/// Contains all data changes relating to entities.
	/// </summary>
	public byte[] PartialEntityData { get; private set; } = Array.Empty<byte>();

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="MultiEntityUpdateMessage"/> with the partial changes of all the entities that changed.
	/// </summary>
	/// <param name="partialEntityData">The partial changes of all the entities that changed.</param>
	public MultiEntityUpdateMessage( byte[] partialEntityData )
	{
		PartialEntityData = partialEntityData;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="MultiEntityUpdateMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		PartialEntityData = new byte[reader.ReadInt32()];
		_ = reader.Read( PartialEntityData, 0, PartialEntityData.Length );
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="MultiEntityUpdateMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( PartialEntityData.Length );
		writer.Write( PartialEntityData );
#endif
	}
}
