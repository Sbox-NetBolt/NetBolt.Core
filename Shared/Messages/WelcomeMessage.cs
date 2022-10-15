using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that signifies to the client that they have joined the server.
/// </summary>
public sealed class WelcomeMessage : NetworkMessage
{
	/// <summary>
	/// The tick rate that the server is running at.
	/// </summary>
	public int TickRate { get; private set; }
	/// <summary>
	/// A welcome message for the client.
	/// </summary>
	public string Message { get; private set; } = "";

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="WelcomeMessage"/> with the servers tick rate and a welcome message to the client.
	/// </summary>
	/// <param name="tickRate"></param>
	/// <param name="message"></param>
	public WelcomeMessage( int tickRate, string message )
	{
		TickRate = tickRate;
		Message = message;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="WelcomeMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		TickRate = reader.ReadInt32();
		Message = reader.ReadString();
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="WelcomeMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( TickRate );
		writer.Write( Message );
#endif
	}

	/// <summary>
	/// Returns a string that represents the <see cref="WelcomeMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="WelcomeMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( WelcomeMessage );
	}
}
