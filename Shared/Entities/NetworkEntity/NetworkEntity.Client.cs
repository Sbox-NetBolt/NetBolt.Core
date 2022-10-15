#if CLIENT
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Entities;

public partial class NetworkEntity
{
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkEntity"/> with a unique network identifier.
	/// </summary>
	/// <param name="networkId">A unique network identifier.</param>
	public NetworkEntity( int networkId ) : base( networkId )
	{
		IEntity.AllEntities.Add( this );
	}

	/// <summary>
	/// <see cref="Update"/> but for the client realm.
	/// </summary>
	protected virtual void UpdateClient()
	{
	}

	internal override void LerpNetworkables( float fraction )
	{
		foreach ( var (propertyName, values) in ClLerpBucket )
		{
			if ( values.Item1 is null || values.Item2 is null )
				continue;

			var property = PropertyNameCache[propertyName];
			if ( property.GetCustomAttribute<ClientAuthorityAttribute>() is not null && Owner == INetworkClient.Local )
				continue;

			var value = property.GetValue( this ) as INetworkable;
			value?.Lerp( fraction, values.Item1, values.Item2 );
			if ( TypeHelper.IsStruct( property.PropertyType ) )
				property.SetValue( this, value );
		}
	}
}
#endif
