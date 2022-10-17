#if CLIENT
using NetBolt.Client;
using Sandbox;
#endif

namespace NetBolt.Shared.Entities;

/// <summary>
/// A test class for player input and basic entity networking.
/// </summary>
public class BasePlayer : NetworkEntity
{
#if CLIENT
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

	protected override void UpdateClient()
	{
		base.UpdateClient();

		if ( Local.Client.Pawn != _player )
		{
			_player.Position = Position;
			_player.Rotation = Rotation;
			return;
		}
		
		if ( Vector3.DistanceBetweenSquared( Position, _player.Position ) > 1 )
			Position = _player.Position;
		
		if ( Rotation.Value != _player.EyeRotation )
			Rotation = _player.EyeRotation;
	}

	protected override void OnDeleted()
	{
		base.OnDeleted();

		_player.Delete();

		if ( Local.Client is not null && Local.Client.Pawn == _player )
			Local.Client.Pawn = null;
	}

	protected override void OnOwnerChanged( INetworkClient? oldOwner, INetworkClient? newOwner )
	{
		base.OnOwnerChanged( oldOwner, newOwner );

		if ( oldOwner == INetworkClient.Local )
			Local.Client.Pawn = null;

		if ( newOwner == INetworkClient.Local )
			Local.Client.Pawn = _player;
	}
#endif
}
