using System.Collections.Generic;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;
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

	/// <summary>
	/// Initializes a default instance of <see cref="ClientListMessage"/>.
	/// </summary>
	[ClientOnly]
	public ClientListMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ClientListMessage"/> with the clients provided.
	/// </summary>
	/// <param name="clients">The clients to notify a client about.</param>
	[ServerOnly]
	public ClientListMessage( IReadOnlyList<INetworkClient> clients )
	{
		Clients = clients;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientListMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		var list = new List<INetworkClient> { Capacity = reader.ReadInt32() };
		for ( var i = 0; i < list.Capacity; i++ )
		{
			var clientId = reader.ReadInt64();
			var isBot = reader.ReadBoolean();

			var client = isBot ? IGlue.Instance.GetBot( clientId ) : IGlue.Instance.GetClient( clientId );
			if ( reader.ReadBoolean() )
				client.Pawn = IEntity.GetEntityById( reader.ReadInt32() );

			list.Add( client );
		}

		Clients = list;
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientListMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
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
