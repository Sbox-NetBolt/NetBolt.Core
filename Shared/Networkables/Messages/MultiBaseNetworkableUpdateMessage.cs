using System;
using System.Collections.Generic;
using System.IO;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing information about an <see cref="BaseNetworkable"/> that has updated.
/// </summary>
public sealed class MultiBaseNetworkableUpdateMessage : NetworkMessage
{
	/// <summary>
	/// Contains all data changes relating to <see cref="BaseNetworkable"/>s.
	/// </summary>
	public byte[] PartialBaseNetworkableData { get; private set; } = Array.Empty<byte>();

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="MultiBaseNetworkableUpdateMessage"/> with all the <see cref="BaseNetworkable"/>s that changed.
	/// </summary>
	/// <param name="changedBaseNetworkables">The <see cref="BaseNetworkable"/>s that changed.</param>
	public MultiBaseNetworkableUpdateMessage( IReadOnlyList<BaseNetworkable> changedBaseNetworkables )
	{
		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );
		writer.Write( changedBaseNetworkables.Count );

		foreach ( var baseNetworkable in changedBaseNetworkables )
		{
			writer.Write( baseNetworkable.NetworkId );
			baseNetworkable.SerializeChanges( writer );
		}
		writer.Close();

		PartialBaseNetworkableData = stream.ToArray();
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="MultiBaseNetworkableUpdateMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		PartialBaseNetworkableData = new byte[reader.ReadInt32()];
		_ = reader.Read( PartialBaseNetworkableData, 0, PartialBaseNetworkableData.Length );
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="MultiBaseNetworkableUpdateMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( PartialBaseNetworkableData.Length );
		writer.Write( PartialBaseNetworkableData );
#endif
	}

	/// <summary>
	/// Returns a string that represents the <see cref="MultiBaseNetworkableUpdateMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="MultiBaseNetworkableUpdateMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( MultiBaseNetworkableUpdateMessage );
	}
}
