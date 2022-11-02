using NetBolt.Shared.Networkables;

namespace NetBolt.Shared.Entities;

/// <summary>
/// A component of an entity.
/// </summary>
// TODO: Revert to EntityComponent once https://github.com/sboxgame/issues/issues/2413 is fixed.
public class EntityComponizzle : ComplexNetworkable
{
	/// <summary>
	/// The entity that this component is a part of.
	/// </summary>
	public NetworkEntity Entity { get; set; } = null!;

	/// <summary>
	/// Invoked when the component is added to an entity.
	/// </summary>
	public virtual void OnAdded()
	{
	}

	/// <summary>
	/// Invoked when the component is removed from an entity.
	/// </summary>
	public virtual void OnRemoved()
	{
	}
}
