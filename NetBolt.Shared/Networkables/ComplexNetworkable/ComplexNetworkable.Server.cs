using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables;

public partial class ComplexNetworkable
{
	/// <summary>
	/// The next unique identifier to give to a <see cref="ComplexNetworkable"/>.
	/// </summary>
	[ServerOnly]
	private static int _nextNetworkId = 1;

	/// <summary>
	/// Gets a new <see cref="NetworkId"/> and steps the internal counter.
	/// </summary>
	/// <returns>A unique network identifier.</returns>
	[ServerOnly]
	private static int StepNextId()
	{
		return _nextNetworkId++;
	}
}
