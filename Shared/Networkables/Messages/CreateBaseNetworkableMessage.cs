using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains information to create a new <see cref="BaseNetworkable"/>.
/// </summary>
public sealed class CreateBaseNetworkableMessage : NetworkMessage
{
	/// <summary>
	/// The class name of the <see cref="BaseNetworkable"/>.
	/// </summary>
	public string BaseNetworkableClass { get; private set; } = "";
	/// <summary>
	/// The unique identifier the <see cref="BaseNetworkable"/> has.
	/// </summary>
	public int NetworkId { get; private set; }

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="CreateBaseNetworkableMessage"/> with the <see cref="BaseNetworkable"/> that is being created.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> that is being created.</param>
	public CreateBaseNetworkableMessage( BaseNetworkable baseNetworkable )
	{
		BaseNetworkableClass = baseNetworkable.GetType().Name;
		NetworkId = baseNetworkable.NetworkId;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="CreateBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		BaseNetworkableClass = reader.ReadString();
		NetworkId = reader.ReadInt32();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="CreateBaseNetworkableMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( BaseNetworkableClass );
		writer.Write( NetworkId );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="CreateBaseNetworkableMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="CreateBaseNetworkableMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( CreateBaseNetworkableMessage );
	}
}
