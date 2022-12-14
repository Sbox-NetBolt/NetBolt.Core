using NetBolt.Shared.Clients;
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

	/// <summary>
	/// Initializes a default instance of <see cref="ClientPawnUpdateMessage"/>.
	/// </summary>
	[ServerOnly]
	public ClientPawnUpdateMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ClientPawnUpdateMessage"/> with the partial data that has changed in the local clients pawn.
	/// </summary>
	/// <param name="partialPawnData">The partial data that has changed in the local clients pawn.</param>
	[ClientOnly]
	public ClientPawnUpdateMessage( byte[] partialPawnData )
	{
		PartialPawnData = partialPawnData;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientPawnUpdateMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ServerOnly]
	public override void Deserialize( NetworkReader reader )
	{
		PartialPawnData = new byte[reader.ReadInt32()];
		_ = reader.Read( PartialPawnData, 0, PartialPawnData.Length );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientPawnUpdateMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ClientOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( PartialPawnData.Length );
		writer.Write( PartialPawnData );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="ClientPawnUpdateMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="ClientPawnUpdateMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( ClientPawnUpdateMessage );
	}
}
