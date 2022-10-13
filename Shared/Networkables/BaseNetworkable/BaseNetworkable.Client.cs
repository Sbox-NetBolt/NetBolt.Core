#if CLIENT
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetBolt.Shared.Utility;
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
	protected Dictionary<string, PropertyDescription> PropertyNameCache { get; } = new();

	/// <summary>
	/// Initializes a new instance of <see cref="BaseNetworkable"/> with a unique network identifier.
	/// </summary>
	/// <param name="networkId">A unique identifier.</param>
	protected BaseNetworkable( int networkId )
	{
		NetworkId = networkId;

		foreach ( var property in TypeHelper.GetAllProperties( GetType() )
					 .Where( property => property.PropertyType.IsAssignableTo( typeof( INetworkable ) ) ) )
		{
			if ( property.GetCustomAttribute<NoNetworkAttribute>() is not null )
				continue;

			PropertyNameCache.Add( property.Name, property );
		}

		AllNetworkables.Add( this );
	}

	/// <summary>
	/// Processes any of the missing <see cref="BaseNetworkable"/>s that could not be found at deserialization time.
	/// </summary>
	internal void ProcessPendingNetworkables()
	{
		foreach ( var (networkId, propertyName) in ClPendingNetworkables )
		{
			var baseNetworkable = All.FirstOrDefault( baseNetworkable => baseNetworkable.NetworkId == networkId );
			if ( baseNetworkable is null )
			{
				Log.Error( $"Tried to process a non-existant {nameof(BaseNetworkable)} ID: {networkId}, Property: {propertyName}" );
				continue;
			}
			
			PropertyNameCache[propertyName].SetValue( this, baseNetworkable );
		}
		
		ClPendingNetworkables.Clear();
	}
}
#endif
