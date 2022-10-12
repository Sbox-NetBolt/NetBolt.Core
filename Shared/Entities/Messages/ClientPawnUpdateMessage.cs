using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A client to server <see cref="NetworkMessage"/> containing information about updates to a <see cref="INetworkClient"/>s pawn.
/// </summary>
public sealed class ClientPawnUpdateMessage : NetworkMessage
{
	/// <summary>
	/// Contains all data changes for the clients pawn.
	/// </summary>
	public byte[] PartialPawnData { get; private set; } = null!;

#if CLIENT
	public ClientPawnUpdateMessage( byte[] partialPawnData )
	{
		PartialPawnData = partialPawnData;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientPawnUpdateMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		PartialPawnData = new byte[reader.ReadInt32()];
		_ = reader.Read( PartialPawnData, 0, PartialPawnData.Length );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientPawnUpdateMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( PartialPawnData.Length );
		writer.Write( PartialPawnData );
	}
}
