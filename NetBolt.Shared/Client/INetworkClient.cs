using System.Collections.Generic;
using System.Linq;
using NetBolt.Shared.Entities;

namespace NetBolt.Shared.Clients;

/// <summary>
/// Contract to define something that is a client that can connect to a server.
/// </summary>
public interface INetworkClient
{
	/// <summary>
	/// The delegate for handling when <see cref="INetworkClient.PawnChanged"/> has been invoked.
	/// </summary>
	delegate void PawnChangedEventHandler( INetworkClient client, IEntity? oldPawn, IEntity? newPawn );
	/// <summary>
	/// Called when a clients pawn has changed.
	/// </summary>
	event PawnChangedEventHandler? PawnChanged;

	/// <summary>
	/// The unique identifier of the client.
	/// </summary>
	long ClientId { get; }

	/// <summary>
	/// Whether or not the client is a bot.
	/// </summary>
	bool IsBot { get; }

	/// <summary>
	/// The player entity that the client is controlling.
	/// </summary>
	IEntity? Pawn { get; set; }

	/// <summary>
	/// Contains all currently connected players in the server.
	/// <remarks>This contains all bots as well.</remarks>
	/// </summary>
	public static IReadOnlyList<INetworkClient> All => IGlue.Instance.Clients;

	/// <summary>
	/// The local client.
	/// </summary>
	public static INetworkClient Local => IGlue.Instance.LocalClient;

	/// <summary>
	/// Gets a client by its unique identifier.
	/// </summary>
	/// <param name="clientId">The unique identifier of the client.</param>
	/// <returns>An <see cref="INetworkClient"/> if found. Null if not</returns>
	public static INetworkClient? GetClientById( long clientId )
	{
		return All.FirstOrDefault( client => client.ClientId == clientId );
	}
}
