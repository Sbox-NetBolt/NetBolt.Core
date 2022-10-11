using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetBolt.Shared;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Utility;
using NetBolt.WebSocket.Enums;

namespace NetBolt.Server;

/// <summary>
/// A bot client.
/// </summary>
public sealed partial class BotClient : INetworkClient
{
	public event INetworkClient.PawnChangedEventHandler? PawnChanged;

	public long ClientId { get; }
	public bool IsBot => true;

	public bool Connected => true;
	public bool ConnectedAndUpgraded => true;
	public int Ping => 0;

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
	private IEntity? _pawn;

	internal BotClient( long clientId )
	{
		ClientId = clientId;
	}

	public override string ToString()
	{
		return $"Bot (ID: {ClientId})";
	}
}
