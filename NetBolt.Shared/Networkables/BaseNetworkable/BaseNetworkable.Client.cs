using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables;

public partial class BaseNetworkable
{
	/// <summary>
	/// An internal map of <see cref="BaseNetworkable"/> identifiers that were not accessible at the time and need setting after de-serializing all <see cref="BaseNetworkable"/>s.
	/// </summary>
	[ClientOnly]
	internal Dictionary<int, string> PendingNetworkables { get; } = new();

	/// <summary>
	/// A dictionary to contain lerped networkables.
	/// </summary>
	[ClientOnly]
	internal readonly Dictionary<string, (INetworkable?, INetworkable?)> LerpBucket = new();

	/// <summary>
	/// Lerps all networkables that are marked with the <see cref="LerpAttribute"/>
	/// </summary>
	[ClientOnly]
	internal virtual void LerpNetworkables( float fraction )
	{
		foreach ( var (propertyName, values) in LerpBucket )
		{
			if ( values.Item1 is null || values.Item2 is null )
				continue;

			var property = PropertyNameCache[propertyName];
			var value = property.GetValue( this ) as INetworkable;
			value?.Lerp( fraction, values.Item1, values.Item2 );
			if ( IGlue.Instance.TypeLibrary.IsStruct( property.PropertyType ) )
				property.SetValue( this, value );
		}
	}

	/// <summary>
	/// Processes any of the missing <see cref="BaseNetworkable"/>s that could not be found at deserialization time.
	/// </summary>
	[ClientOnly]
	internal void ProcessPendingNetworkables()
	{
		foreach ( var (networkId, propertyName) in PendingNetworkables )
		{
			var baseNetworkable = All.FirstOrDefault( baseNetworkable => baseNetworkable.NetworkId == networkId );
			if ( baseNetworkable is null )
			{
				IGlue.Instance.Logger.Error( $"Tried to process a non-existant {nameof( BaseNetworkable )} ID: {networkId}, Property: {propertyName}" );
				continue;
			}

			PropertyNameCache[propertyName].SetValue( this, baseNetworkable );
		}

		PendingNetworkables.Clear();
	}
}
