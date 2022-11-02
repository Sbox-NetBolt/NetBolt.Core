using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains a new <see cref="ComplexNetworkable"/>.
/// </summary>
public sealed class CreateComplexNetworkableMessage : NetworkMessage
{
	/// <summary>
	/// The <see cref="Networkables.ComplexNetworkable"/> that was created.
	/// </summary>
	public ComplexNetworkable ComplexNetworkable { get; private set; } = null!;

	/// <summary>
	/// Initializes a default instance of <see cref="CreateComplexNetworkableMessage"/>.
	/// </summary>
	[ClientOnly]
	public CreateComplexNetworkableMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="CreateComplexNetworkableMessage"/> with the <see cref="ComplexNetworkable"/> that is being created.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> that is being created.</param>
	[ServerOnly]
	public CreateComplexNetworkableMessage( ComplexNetworkable complexNetworkable )
	{
		ComplexNetworkable = complexNetworkable;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="CreateComplexNetworkableMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		ComplexNetworkable = reader.ReadNetworkable<ComplexNetworkable>();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="CreateComplexNetworkableMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( ComplexNetworkable );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="CreateComplexNetworkableMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="CreateComplexNetworkableMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( CreateComplexNetworkableMessage );
	}
}
