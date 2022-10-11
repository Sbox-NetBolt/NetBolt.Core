#if CLIENT
namespace NetBolt.Shared;

public partial class NetworkClient
{
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkClient"/> with a unique client identifier.
	/// </summary>
	internal NetworkClient( long clientId )
	{
		ClientId = clientId;
	}
}
#endif
