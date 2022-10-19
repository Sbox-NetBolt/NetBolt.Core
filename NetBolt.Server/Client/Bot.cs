using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.WebSocket.Enums;
using System;
using System.Threading.Tasks;

namespace NetBolt.Server;

/// <summary>
/// Represents a non-player controlled bot in the server.
/// </summary>
internal class Bot : IClient
{
	/// <inheritdoc/>
	public event INetworkClient.PawnChangedEventHandler? PawnChanged;

	/// <inheritdoc/>
	public long ClientId { get; }

	/// <inheritdoc/>
	public bool IsBot => true;

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

	/// <inheritdoc/>
	public bool Connected => true;

	/// <inheritdoc/>
	public bool ConnectedAndUpgraded => true;

	/// <inheritdoc/>
	public int Ping => 0;

	/// <summary>
	/// Initializes a new instance of  <see cref="Bot"/> with a unique identifier.
	/// </summary>
	/// <param name="botId">The unique identifier of the bot.</param>
	internal Bot( long botId )
	{
		ClientId = botId;
	}

	/// <inheritdoc/>
	public Task DisconnectAsync( WebSocketDisconnectReason reason = WebSocketDisconnectReason.Requested, string strReason = "", WebSocketError? error = null )
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc/>
	public Task HandleAsync()
	{
		return Task.CompletedTask;
	}

	/// <inheritdoc/>
	public ValueTask<int> PingAsync( int timeout = int.MaxValue )
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc/>
	public void QueueSend( NetworkMessage message )
	{
		throw new System.NotImplementedException();
	}

	/// <inheritdoc/>
	public void QueueSend( byte[] bytes )
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc/>
	public void QueueSend( string message )
	{
		throw new NotSupportedException();
	}
}
