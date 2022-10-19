using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Utility;
using NetBolt.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetBolt.Server;

/// <summary>
/// Represents a player in the server.
/// </summary>
internal class Client : WebSocketClient, IClient
{
	/// <inheritdoc/>
	public event INetworkClient.PawnChangedEventHandler? PawnChanged;

	/// <inheritdoc/>
	public long ClientId { get; private set; }

	/// <inheritdoc/>
	public bool IsBot => false;

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
	/// Initializes a new instance of <see cref="Client"/> with the clients underlying socket and the server it is a part of.
	/// </summary>
	/// <param name="socket">The underlying socket of the client.</param>
	/// <param name="server">The server the client is a part of.</param>
	internal Client( TcpClient socket, IWebSocketServer server ) : base( socket, server )
	{
	}

	/// <inheritdoc/>
	public void QueueSend( NetworkMessage message )
	{
		var stream = new MemoryStream();
		var writer = new NetworkWriter( stream );
		writer.WriteNetworkable( message );
		writer.Close();

		QueueSend( stream.ToArray() );
	}

	/// <inheritdoc/>
	protected override void OnData( ReadOnlySpan<byte> bytes )
	{
		base.OnData( bytes );

		var reader = new NetworkReader( new MemoryStream( bytes.ToArray() ) );
		var message = NetworkMessage.DeserializeMessage( reader );
		reader.Close();

		GameServer.Instance.QueueIncoming( this, message );
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> VerifyHandshake( IReadOnlyDictionary<string, string> headers, string request )
	{
		if ( !headers.TryGetValue( "Steam", out var clientIdStr ) )
			return new ValueTask<bool>( false );

		if ( !long.TryParse( clientIdStr, out var clientId ) )
			return new ValueTask<bool>( false );

		if ( GameServer.Instance.GetClientById( clientId ) is not null )
			return new ValueTask<bool>( false );

		ClientId = clientId;
		return base.VerifyHandshake( headers, request );
	}
}
