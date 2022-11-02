using System.Linq;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> to notify some clients when a client has had its pawn changed.
/// </summary>
public sealed class ClientPawnChangedMessage : NetworkMessage
{
	/// <summary>
	/// The <see cref="INetworkClient"/> that has had its <see cref="INetworkClient.Pawn"/> changed.
	/// </summary>
	public INetworkClient Client { get; private set; } = null!;
	/// <summary>
	/// The old <see cref="IEntity"/> the <see cref="Client"/> was controlling.
	/// </summary>
	public IEntity? OldPawn { get; private set; }
	/// <summary>
	/// The new <see cref="IEntity"/> the <see cref="Client"/> is controlling.
	/// </summary>
	public IEntity? NewPawn { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="ClientPawnChangedMessage"/>.
	/// </summary>
	[ClientOnly]
	public ClientPawnChangedMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ClientPawnChangedMessage"/> with the information provided.
	/// </summary>
	/// <param name="client">The client whose pawn has changed.</param>
	/// <param name="oldEntity">The old pawn the client was controlling.</param>
	/// <param name="newEntity">The new pawn the client is controlling.</param>
	[ServerOnly]
	public ClientPawnChangedMessage( INetworkClient client, IEntity? oldEntity, IEntity? newEntity )
	{
		Client = client;
		OldPawn = oldEntity;
		NewPawn = newEntity;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="ClientPawnChangedMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		var clientId = reader.ReadInt64();
		var client = INetworkClient.All.FirstOrDefault( client => client.ClientId == clientId );
		if ( client is null )
		{
			ILogger.Instance.Error( "Failed to get client with ID \"{0}\"", clientId );
			return;
		}

		Client = client;
		if ( reader.ReadBoolean() )
			OldPawn = IEntity.GetOrRequestById( reader.ReadInt32(), entity => OldPawn = entity );
		if ( reader.ReadBoolean() )
			NewPawn = IEntity.GetOrRequestById( reader.ReadInt32(), entity => NewPawn = entity );
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ClientPawnChangedMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( Client.ClientId );

		var hasOldPawn = OldPawn is not null;
		writer.Write( hasOldPawn );
		if ( hasOldPawn )
			writer.Write( OldPawn!.EntityId );

		var hasNewPawn = NewPawn is not null;
		writer.Write( hasNewPawn );
		if ( hasNewPawn )
			writer.Write( NewPawn!.EntityId );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="ClientPawnChangedMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="ClientPawnChangedMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( ClientPawnChangedMessage );
	}
}
