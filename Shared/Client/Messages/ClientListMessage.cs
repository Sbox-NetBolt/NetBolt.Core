using System.Collections.Generic;
#if CLIENT
using NetBolt.Shared.Entities;
#endif
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a list of <see cref="INetworkClient"/>s to notify the client about.
/// </summary>
public sealed class ClientListMessage : NetworkMessage
{
	/// <summary>
	/// Contains all clients to notify the client about.
	/// </summary>
	public IReadOnlyList<INetworkClient> Clients { get; private set; } = null!;

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="ClientListMessage"/> with the clients provided.
	/// </summary>
	/// <param name="clients">The clients to notify a client about.</param>
	public ClientListMessage( IReadOnlyList<INetworkClient> clients )
	{
		Clients = clients;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientListMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		var list = new List<INetworkClient> {Capacity = reader.ReadInt32()};
		for ( var i = 0; i < list.Capacity; i++ )
		{
			var clientId = reader.ReadInt64();
			var isBot = reader.ReadBoolean();

			INetworkClient client = isBot ? new BotClient( clientId ) : new NetworkClient( clientId );
			if ( reader.ReadBoolean() )
				client.Pawn = IEntity.GetEntityById(reader.ReadInt32());
			
			list.Add( client );
		}

		Clients = list;
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientListMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( Clients.Count );
		foreach ( var client in Clients )
		{
			writer.Write( client.ClientId );
			writer.Write( client.IsBot );

			var hasPawn = client.Pawn is not null;
			writer.Write( hasPawn );
			if ( hasPawn )
				writer.Write( client.Pawn!.EntityId );
		}
#endif
	}

	/// <summary>
	/// Returns a string that represents the <see cref="ClientListMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="ClientListMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( ClientListMessage );
	}
}
