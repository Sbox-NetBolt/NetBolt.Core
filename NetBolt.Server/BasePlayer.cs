using NetBolt.Server;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Networkables.Builtin;
using NetBolt.Shared.RemoteProcedureCalls;
using System;

namespace NetBolt.Shared;

/// <summary>
/// A test class for player input and basic entity networking.
/// </summary>
public class BasePlayer : NetworkEntity
{
	/// <summary>
	/// Adds the <see cref="ColorComponent"/> and changes the color.
	/// </summary>
	[Rpc.Server]
	public void RotateColorComponent()
	{
		var component = Components.AddOrGetComponent<ColorComponent>();
		var oldColor = component.Color;
		component.Color = new NetworkedVector3( Random.Shared.Next( 0, 255 ), Random.Shared.Next( 0, 255 ), Random.Shared.Next( 0, 255 ) );
		Log.Info( "Component color changed from {0} to {1}", oldColor, component.Color );
	}
}
