using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;

namespace NetBolt.Client;

/// <summary>
/// Represents a client in a server.
/// </summary>
internal class NetworkClient : INetworkClient
{
	/// <inheritdoc/>
	public event INetworkClient.PawnChangedEventHandler? PawnChanged;

	/// <inheritdoc/>
	public long ClientId { get; }

	/// <inheritdoc/>
	public bool IsBot { get; }

	/// <inheritdoc/>
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
	/// Initializes a new instance of <see cref="NetworkClient"/> with a unique identifier and whether or not it is a bot.
	/// </summary>
	/// <param name="clientId">The unique identifier of the client.</param>
	/// <param name="isBot">Whether or not the client is a bot.</param>
	internal NetworkClient( long clientId, bool isBot )
	{
		ClientId = clientId;
		IsBot = isBot;
	}

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkClient"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkClient"/>.</returns>
	public override string ToString()
	{
		return $"{(IsBot ? "Bot" : "Client")} (ID: {ClientId})";
	}
}
