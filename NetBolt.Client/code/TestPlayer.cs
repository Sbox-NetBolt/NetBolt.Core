using NetBolt.Shared.Entities;
using Sandbox;

namespace NetBolt.Client;

/// <summary>
/// A Sbox test player to be linked to a <see cref="NetworkEntity"/>.
/// </summary>
public sealed class TestPlayer : AnimatedEntity
{
	/// <summary>
	/// The grubs movement controller.
	/// </summary>
	private BasePlayerController Controller { get; set; } = null!;

	/// <summary>
	/// The camera that the team client will see the game through.
	/// </summary>
	private CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		Camera = new FirstPersonCamera();
		Controller = new WalkController();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	/// <inheritdoc/>
	public override void Simulate( Sandbox.Client cl )
	{
		base.Simulate( cl );

		Controller.Simulate( cl, this, null );

		if ( Input.Pressed( InputButton.View ) )
			Camera = Camera is FirstPersonCamera ? new ThirdPersonCamera() : new FirstPersonCamera();
	}

	/// <inheritdoc/>
	public override void FrameSimulate( Sandbox.Client cl )
	{
		base.FrameSimulate( cl );

		// Update rotation every frame, to keep things smooth
		Rotation = Input.Rotation;
		EyeRotation = Rotation;
	}
}
