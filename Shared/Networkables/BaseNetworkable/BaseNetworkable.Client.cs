#if CLIENT
using System.Collections.Generic;
using System.Reflection;
using Sandbox;

namespace NetBolt.Shared.Networkables;

public partial class BaseNetworkable
{
	/// <summary>
	/// An internal map of <see cref="BaseNetworkable"/> identifiers that were not accessible at the time and need setting after de-serializing all <see cref="BaseNetworkable"/>s.
	/// </summary>
	internal Dictionary<int, string> ClPendingNetworkables { get; } = new();

	/// <summary>
	/// A <see cref="PropertyInfo"/> cache of all networked properties.
	/// </summary>
	protected Dictionary<string, PropertyDescription> PropertyNameCache { get;} = new();

	/// <summary>
	/// Initializes a new instance of <see cref="BaseNetworkable"/> with a unique network identifier.
	/// </summary>
	/// <param name="networkId">A unique identifier.</param>
	protected BaseNetworkable( int networkId )
	{
	}
}
#endif
