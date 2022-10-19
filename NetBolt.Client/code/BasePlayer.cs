using NetBolt.Client;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Networkables.Builtin;
using Sandbox;
using System.Numerics;

namespace NetBolt.Shared;

public class BasePlayer : NetworkEntity
{
	/// <summary>
	/// The Sbox entity of the player.
	/// </summary>
	private readonly TestPlayer _player;

	/// <summary>
	/// Initializes a new instance of <see cref="BasePlayer"/>.
	/// </summary>
	public BasePlayer()
	{
		_player = new TestPlayer();
	}

	/// <inheritdoc/>
	protected override void UpdateClient()
	{
		base.UpdateClient();

		if ( Local.Client.Pawn != _player )
		{
			_player.Position = Position.Value;
			_player.Rotation = Rotation.Value;
			return;
		}

		if ( Vector3.DistanceBetweenSquared( Position.Value, _player.Position ) > 1 )
			Position = new NetworkedVector3( _player.Position );

		if ( Rotation.Value != (Quaternion)_player.EyeRotation )
			Rotation = new NetworkedQuaternion( _player.EyeRotation );
	}

	/// <inheritdoc/>
	protected override void OnDeleted()
	{
		base.OnDeleted();

		_player.Delete();

		if ( Local.Client is not null && Local.Client.Pawn == _player )
			Local.Client.Pawn = null;
	}

	/// <inheritdoc/>
	protected override void OnOwnerChanged( INetworkClient? oldOwner, INetworkClient? newOwner )
	{
		base.OnOwnerChanged( oldOwner, newOwner );

		if ( oldOwner == INetworkClient.Local )
			Local.Client.Pawn = null;

		if ( newOwner == INetworkClient.Local )
			Local.Client.Pawn = _player;
	}
}
