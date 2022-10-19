using NetBolt.Shared.Clients;
using NetBolt.Shared.Networkables;

namespace NetBolt.Shared.Entities;

public partial class NetworkEntity
{
	/// <summary>
	/// <see cref="Update"/> but for the client realm.
	/// </summary>
	protected virtual void UpdateClient()
	{
	}

	/// <inheritdoc/>
	internal override void LerpNetworkables( float fraction )
	{
		foreach ( var (propertyName, values) in LerpBucket )
		{
			if ( values.Item1 is null || values.Item2 is null )
				continue;

			var property = PropertyNameCache[propertyName];
			if ( property.HasAttribute<ClientAuthorityAttribute>() && Owner == INetworkClient.Local )
				continue;

			var value = property.GetValue( this ) as INetworkable;
			value?.Lerp( fraction, values.Item1, values.Item2 );
			if ( IGlue.Instance.TypeLibrary.IsStruct( property.PropertyType ) )
				property.SetValue( this, value );
		}
	}
}
