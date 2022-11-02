using System;
using System.Collections.Generic;
using System.IO;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing information about an <see cref="ComplexNetworkable"/> that has updated.
/// </summary>
public sealed class MultiComplexNetworkableUpdateMessage : NetworkMessage
{
	/// <summary>
	/// Contains all data changes relating to <see cref="ComplexNetworkable"/>s.
	/// </summary>
	public byte[] PartialComplexNetworkableData { get; private set; } = Array.Empty<byte>();

	/// <summary>
	/// Initializes a default instance of <see cref="MultiComplexNetworkableUpdateMessage"/>.
	/// </summary>
	[ClientOnly]
	public MultiComplexNetworkableUpdateMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="MultiComplexNetworkableUpdateMessage"/> with all the <see cref="ComplexNetworkable"/>s that changed.
	/// </summary>
	/// <param name="changedComplexNetworkables">The <see cref="ComplexNetworkable"/>s that changed.</param>
	[ServerOnly]
	public MultiComplexNetworkableUpdateMessage( IReadOnlyList<ComplexNetworkable> changedComplexNetworkables )
	{
		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );
		writer.Write( changedComplexNetworkables.Count );

		foreach ( var complexNetworkable in changedComplexNetworkables )
		{
			writer.Write( complexNetworkable.NetworkId );
			complexNetworkable.SerializeChanges( writer );
		}
		writer.Close();

		PartialComplexNetworkableData = stream.ToArray();
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="MultiComplexNetworkableUpdateMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		PartialComplexNetworkableData = new byte[reader.ReadInt32()];
		_ = reader.Read( PartialComplexNetworkableData, 0, PartialComplexNetworkableData.Length );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="MultiComplexNetworkableUpdateMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( PartialComplexNetworkableData.Length );
		writer.Write( PartialComplexNetworkableData );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="MultiComplexNetworkableUpdateMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="MultiComplexNetworkableUpdateMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( MultiComplexNetworkableUpdateMessage );
	}
}
