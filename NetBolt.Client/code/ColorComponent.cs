using NetBolt.Shared.Entities;
using NetBolt.Shared.Networkables.Builtin;
using NetBolt.Shared.RemoteProcedureCalls;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared;

/// <summary>
/// Represents a component with a networkable color parameters.
/// </summary>
public class ColorComponent : EntityComponizzle
{
	/// <summary>
	/// The color in vector form.
	/// </summary>
	public NetworkedVector3 Color { get; set; } = System.Numerics.Vector3.Zero;

	/// <summary>
	/// Sends an RPC to the server to change the color.
	/// </summary>
	[ClientOnly]
	public void RotateColorRpc()
	{
		this.CallRpc( "RotateColorRpc" );
	}
}
