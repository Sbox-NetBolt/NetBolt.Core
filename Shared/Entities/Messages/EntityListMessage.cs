#if CLIENT
using System.Buffers;
#endif
using System.Collections.Generic;
using NetBolt.Shared.Entities;
#if SERVER
using System.IO;
#endif
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a list of <see cref="IEntity"/>s to notify the client about.
/// </summary>
public sealed class EntityListMessage : NetworkMessage
{
	/// <summary>
	/// The data of all <see cref="IEntity"/>s passed.
	/// </summary>
	public List<byte[]> EntityData { get; private set; } = new();

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="EntityListMessage"/> with the list of entities to notify a client about.
	/// </summary>
	/// <param name="entityList">The list of entities to notify a client about.</param>
	public EntityListMessage( IEnumerable<IEntity> entityList )
	{
		EntityData = new List<byte[]>();
		foreach ( var entity in entityList )
		{
			var stream = new MemoryStream();
			var writer = new NetworkWriter( stream );
			writer.WriteEntity( entity );
			writer.Close();
			EntityData.Add( stream.ToArray() );
		}
	}
#endif

#if CLIENT
	/// <summary>
	/// Returns all of the entity data arrays to the shared pool.
	/// </summary>
	~EntityListMessage()
	{
		foreach ( var data in EntityData )
			ArrayPool<byte>.Shared.Return( data, true );
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="EntityListMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		EntityData = new List<byte[]> {Capacity = reader.ReadInt32()};
		for ( var i = 0; i < EntityData.Capacity; i++ )
		{
			var dataLength = reader.ReadInt32();
			var bytes = ArrayPool<byte>.Shared.Rent( dataLength );
			_ = reader.Read( bytes, 0, dataLength );
			EntityData.Add( bytes );
		}
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="EntityListMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( EntityData.Count );
		foreach ( var data in EntityData )
		{
			writer.Write( data.Length );
			writer.Write( data );
		}
#endif
	}
}
