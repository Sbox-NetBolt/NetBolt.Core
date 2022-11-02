using System;
using System.Collections.Generic;
using System.Linq;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables;

public partial class ComplexNetworkable
{
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
			if ( ITypeLibrary.Instance.IsStruct( property.PropertyType ) )
				property.SetValue( this, value );
		}
	}

	/// <summary>
	/// Attempts to get a <see cref="ComplexNetworkable"/> by its unique network identifier. If no <see cref="ComplexNetworkable"/> is found, then a request will be made with the provided callback.
	/// </summary>
	/// <param name="networkId">The unique network identifier of the <see cref="ComplexNetworkable"/>.</param>
	/// <param name="cb">The callback to invoke when the <see cref="ComplexNetworkable"/> exists.</param>
	/// <returns>The <see cref="ComplexNetworkable"/> if found. Null otherwise.</returns>
	[ClientOnly]
	public static ComplexNetworkable? GetOrRequestById( int networkId, Action<ComplexNetworkable> cb )
	{
		if ( networkId == -1 )
			return null;

		var complexNetworkable = All.FirstOrDefault( complexNetworkable => complexNetworkable.NetworkId == networkId );
		if ( complexNetworkable is not null )
			return complexNetworkable;

		INetBoltClient.Instance.RequestBaseNetworkable( networkId, cb );
		return null;
	}
}
