using NetBolt.Shared.Entities;
#if SERVER
using NetBolt.WebSocket;
#endif

namespace NetBolt.Shared;

/// <summary>
/// Base class for any non-bot clients connected to a server.
/// </summary>
#if SERVER
public partial class NetworkClient : WebSocketClient, INetworkClient
#endif
#if CLIENT
public partial class NetworkClient : INetworkClient
#endif
{
	/// <summary>
	/// Occurs when this clients pawn has changed.
	/// </summary>
	public event INetworkClient.PawnChangedEventHandler? PawnChanged;

	/// <summary>
	/// The unique identifier of the client.
	/// </summary>
	public long ClientId { get; private set; }

	/// <summary>
	/// Whether or not this client is a bot (not controlled by a live player).
	/// </summary>
	public bool IsBot => false;

	/// <summary>
	/// The player entity that the client is controlling.
	/// </summary>
	public IEntity? Pawn
	{
		get => _pawn;
		set
		{
			if ( value is not null && _pawn is not null )
				return;

			if ( value is not null && _pawn is not null && value.EntityId == _pawn.EntityId )
				return;

			var oldPawn = _pawn;
			_pawn = value;
			PawnChanged?.Invoke( this, oldPawn, _pawn );
		}
	}
	/// <summary>
	/// See <see cref="Pawn"/>.
	/// </summary>
	private IEntity? _pawn;

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkClient"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkClient"/>.</returns>
	public override string ToString()
	{
		return $"Client (ID: {ClientId})";
	}
}
