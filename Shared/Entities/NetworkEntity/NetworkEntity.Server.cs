#if SERVER
namespace NetBolt.Shared.Entities;

public partial class NetworkEntity
{
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkEntity"/>.
	/// </summary>
	public NetworkEntity()
	{
	}

	/// <summary>
	/// <see cref="Update"/> but for the server realm.
	/// </summary>
	protected virtual void UpdateServer()
	{
	}
}
#endif
