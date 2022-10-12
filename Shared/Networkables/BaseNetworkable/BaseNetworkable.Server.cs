#if SERVER
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables;

public partial class BaseNetworkable
{
	/// <summary>
	/// A <see cref="PropertyInfo"/> cache of all networked properties.
	/// </summary>
	protected readonly Dictionary<string, PropertyInfo> PropertyNameCache = new();

	/// <summary>
	/// Initializes a new instance of <see cref="BaseNetworkable"/>.
	/// </summary>
	protected BaseNetworkable()
	{
		NetworkId = StepNextId();

		foreach ( var property in TypeHelper.GetAllProperties( GetType() )
					 .Where( property => property.PropertyType.IsAssignableTo( typeof( INetworkable ) ) ) )
		{
			if ( property.GetCustomAttribute<NoNetworkAttribute>() is not null )
				continue;

			PropertyNameCache.Add( property.Name, property );
		}

		AllNetworkables.Add( NetworkId, this );
	}

	/// <summary>
	/// The next unique identifier to give to a <see cref="BaseNetworkable"/>.
	/// </summary>
	private static int _nextNetworkId = 1;

	/// <summary>
	/// Gets a new <see cref="NetworkId"/> and steps the internal counter.
	/// </summary>
	/// <returns>A unique network identifier.</returns>
	private static int StepNextId()
	{
		return _nextNetworkId++;
	}
}
#endif
