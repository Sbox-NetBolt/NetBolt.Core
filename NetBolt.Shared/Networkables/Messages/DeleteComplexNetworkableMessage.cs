using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains an <see cref="ComplexNetworkable"/> to delete.
/// </summary>
public sealed class DeleteComplexNetworkableMessage : NetworkMessage
{
	/// <summary>
	/// The unique identifier of the <see cref="ComplexNetworkable"/> to delete.
	/// </summary>
	public int ComplexNetworkableId { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="DeleteComplexNetworkableMessage"/>.
	/// </summary>
	[ClientOnly]
	public DeleteComplexNetworkableMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="DeleteComplexNetworkableMessage"/> with the <see cref="ComplexNetworkable"/> that is being deleted.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> that is being deleted.</param>
	[ServerOnly]
	public DeleteComplexNetworkableMessage( ComplexNetworkable complexNetworkable )
	{
		ComplexNetworkableId = complexNetworkable.NetworkId;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="DeleteComplexNetworkableMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		ComplexNetworkableId = reader.ReadInt32();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="DeleteComplexNetworkableMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( ComplexNetworkableId );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="DeleteComplexNetworkableMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="DeleteComplexNetworkableMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( DeleteComplexNetworkableMessage );
	}
}
