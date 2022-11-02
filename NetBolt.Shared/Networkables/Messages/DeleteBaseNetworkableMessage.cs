using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains an <see cref="ComplexNetworkable"/> to delete.
/// </summary>
public sealed class DeleteBaseNetworkableMessage : NetworkMessage
{
	/// <summary>
	/// The unique identifier of the <see cref="ComplexNetworkable"/> to delete.
	/// </summary>
	public int BaseNetworkableId { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	[ClientOnly]
	public DeleteBaseNetworkableMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="DeleteBaseNetworkableMessage"/> with the <see cref="ComplexNetworkable"/> that is being deleted.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> that is being deleted.</param>
	[ServerOnly]
	public DeleteBaseNetworkableMessage( ComplexNetworkable complexNetworkable )
	{
		BaseNetworkableId = complexNetworkable.NetworkId;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		BaseNetworkableId = reader.ReadInt32();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="DeleteBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( BaseNetworkableId );
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
