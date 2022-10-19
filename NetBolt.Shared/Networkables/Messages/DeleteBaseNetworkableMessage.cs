using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains an <see cref="BaseNetworkable"/> to delete.
/// </summary>
public sealed class DeleteBaseNetworkableMessage : NetworkMessage
{
	/// <summary>
	/// The unique identifier of the <see cref="BaseNetworkable"/> to delete.
	/// </summary>
	public int NetworkId { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	[ClientOnly]
	public DeleteBaseNetworkableMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="DeleteBaseNetworkableMessage"/> with the <see cref="BaseNetworkable"/> that is being deleted.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> that is being deleted.</param>
	[ServerOnly]
	public DeleteBaseNetworkableMessage( BaseNetworkable baseNetworkable )
	{
		NetworkId = baseNetworkable.NetworkId;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		NetworkId = reader.ReadInt32();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( NetworkId );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="DeleteBaseNetworkableMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( DeleteBaseNetworkableMessage );
	}
}
