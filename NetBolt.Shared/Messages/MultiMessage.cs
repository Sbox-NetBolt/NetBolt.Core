using System;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that contains multiple <see cref="NetworkMessage"/>s in itself.
/// </summary>
public sealed class MultiMessage : NetworkMessage
{
	/// <summary>
	/// The <see cref="NetworkMessage"/>s to send.
	/// </summary>
	public NetworkMessage[] Messages { get; private set; } = Array.Empty<NetworkMessage>();

	/// <summary>
	/// Initializes a default instance of <see cref="MultiMessage"/>.
	/// </summary>
	[ClientOnly]
	public MultiMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="MultiMessage"/> with an array of <see cref="NetworkMessage"/>s to send.
	/// </summary>
	/// <param name="messages"></param>
	[ServerOnly]
	public MultiMessage( params NetworkMessage[] messages )
	{
		Messages = messages;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="MultiMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		Messages = new NetworkMessage[reader.ReadInt32()];
		for ( var i = 0; i < Messages.Length; i++ )
			Messages[i] = reader.ReadNetworkable<NetworkMessage>();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="MultiMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( Messages.Length );
		foreach ( var message in Messages )
			writer.Write( message );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="MultiMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="MultiMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( MultiMessage );
	}
}
