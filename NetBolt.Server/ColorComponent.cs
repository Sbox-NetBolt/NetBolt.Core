using NetBolt.Shared.Entities;
using NetBolt.Shared.Networkables.Builtin;
using NetBolt.Shared.RemoteProcedureCalls;
using NetBolt.Shared.Utility;
using System;

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
	/// Changes the color on the component.
	/// </summary>
	[ServerOnly, Rpc.Server]
	public void RotateColorRpc()
	{
		Color = new NetworkedVector3( Random.Shared.NextSingle() * 255, Random.Shared.NextSingle() * 255, Random.Shared.NextSingle() * 255 );
	}
}
