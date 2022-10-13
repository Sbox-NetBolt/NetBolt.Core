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
	/// Initializes a new instance of <see cref="BasePlayer"/> with a unique entity identifier.
	/// </summary>
	/// <param name="entityId">A unique entity identifier.</param>
	public BasePlayer( int entityId ) : base( entityId )
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

		if ( Position.Value != (System.Numerics.Vector3)_player.Position )
			Position = _player.Position;
		
		if ( Rotation.Value != _player.EyeRotation )
			Rotation = _player.EyeRotation;
	}

	public override void Delete()
	{
		base.Delete();

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
