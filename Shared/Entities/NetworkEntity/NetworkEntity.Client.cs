#if CLIENT
namespace NetBolt.Shared.Entities;

public partial class NetworkEntity
{
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkEntity"/> with a unique network identifier.
	/// </summary>
	/// <param name="networkId">A unique network identifier.</param>
	public NetworkEntity( int networkId ) : base( networkId )
	{
	}

	/// <summary>
	/// <see cref="Update"/> but for the client realm.
	/// </summary>
	protected virtual void UpdateClient()
	{
	}
}
#endif
