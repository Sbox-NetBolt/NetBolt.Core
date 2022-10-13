using NetBolt.Shared;
using NetBolt.Shared.Entities;

namespace NetBolt.Server;

/// <summary>
/// A bot client.
/// </summary>
public sealed partial class BotClient : INetworkClient
{
	/// <summary>
	/// Occurs when this clients pawn has changed.
	/// </summary>
	public event INetworkClient.PawnChangedEventHandler? PawnChanged;

	/// <summary>
	/// The unique identifier of the client.
	/// </summary>
	public long ClientId { get; }

	/// <summary>
	/// Whether or not this client is a bot (not controlled by a live player).
	/// </summary>
	public bool IsBot => true;

	/// <summary>
	/// Whether or not this client is connected to the server.
	/// <remarks>This does not mean they are capable of receiving messages yet. See ConnectedAndUpgraded.</remarks>
	/// </summary>
	public bool Connected => true;
	/// <summary>
	/// Whether or not this client is connected to the server and has been upgraded to the web socket protocol.
	/// </summary>
	public bool ConnectedAndUpgraded => true;
	/// <summary>
	/// The clients current ping time in milliseconds.
	/// </summary>
	public int Ping => 0;

	/// <summary>
	/// The player entity that the client is controlling.
	/// </summary>
	public IEntity? Pawn
	{
		get => _pawn;
		set
		{
			if ( _pawn == value )
				return;

			var oldPawn = _pawn;
			if ( oldPawn is not null )
				oldPawn.Owner = null;
			
			_pawn = value;
			if ( _pawn is not null )
				_pawn.Owner = this;
			
			PawnChanged?.Invoke( this, oldPawn, _pawn );
		}
	}
	/// <summary>
	/// See <see cref="Pawn"/>.
	/// </summary>
	private IEntity? _pawn;

	/// <summary>
	/// Initializes a new instance of <see cref="BotClient"/> with a unique client identifier.
	/// </summary>
	/// <param name="clientId">A unique identifier for the client.</param>
	internal BotClient( long clientId )
	{
		ClientId = clientId;
	}

	/// <summary>
	/// Returns a string that represents the <see cref="BotClient"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="BotClient"/>.</returns>
	public override string ToString()
	{
		return $"Bot (ID: {ClientId})";
	}
}
