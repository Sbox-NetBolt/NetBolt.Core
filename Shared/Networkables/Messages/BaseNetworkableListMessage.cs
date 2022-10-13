#if CLIENT
using System.Buffers;
#endif
using System.Collections.Generic;
using NetBolt.Shared.Networkables;
#if SERVER
using System.IO;
#endif
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a list of <see cref="BaseNetworkable"/>s to notify the client about.
/// </summary>
public sealed class BaseNetworkableListMessage : NetworkMessage
{
	/// <summary>
	/// The data of all <see cref="BaseNetworkable"/>s passed.
	/// </summary>
	public List<byte[]> BaseNetworkableData { get; private set; } = new();

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="BaseNetworkableListMessage"/> with the list of <see cref="BaseNetworkable"/> to notify a client about.
	/// </summary>
	/// <param name="baseNetworkableList">The list of <see cref="BaseNetworkable"/> to notify a client about.</param>
	public BaseNetworkableListMessage( IEnumerable<BaseNetworkable> baseNetworkableList )
	{
		BaseNetworkableData = new List<byte[]>();
		foreach ( var baseNetworkable in baseNetworkableList )
		{
			var stream = new MemoryStream();
			var writer = new NetworkWriter( stream );
			writer.WriteBaseNetworkable( baseNetworkable );
			writer.Close();
			BaseNetworkableData.Add( stream.ToArray() );
		}
	}
#endif

#if CLIENT
	/// <summary>
	/// Returns all of the entity data arrays to the shared pool.
	/// </summary>
	~BaseNetworkableListMessage()
	{
		foreach ( var data in BaseNetworkableData )
			ArrayPool<byte>.Shared.Return( data, true );
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		BaseNetworkableData = new List<byte[]> { Capacity = reader.ReadInt32() };

		for ( var i = 0; i < BaseNetworkableData.Capacity; i++ )
		{
			var dataLength = reader.ReadInt32();
			var bytes = ArrayPool<byte>.Shared.Rent( dataLength );
			_ = reader.Read( bytes, 0, dataLength );
			BaseNetworkableData.Add( bytes );
		}
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( BaseNetworkableData.Count );
		foreach ( var data in BaseNetworkableData )
		{
			writer.Write( data.Length );
			writer.Write( data );
		}
#endif
	}
	
	/// <summary>
	/// Returns a string that represents the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="BaseNetworkableListMessage"/>.</returns>
	public override string ToString()
	{
		return nameof(BaseNetworkableListMessage);
	}
}
