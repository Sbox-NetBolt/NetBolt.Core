using System.Linq;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Networkables.Builtin;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Entities;

/// <summary>
/// The base class for all of your entity needs.
/// </summary>
public partial class NetworkEntity : BaseNetworkable, IEntity
{
	/// <summary>
	/// The unique identifier of the <see cref="NetworkEntity"/>.
	/// </summary>
	public int EntityId => NetworkId;

	/// <summary>
	/// The <see cref="INetworkClient"/> that owns this <see cref="IEntity"/>.
	/// </summary>
	public INetworkClient? Owner
	{
		get => _owner;
		set
		{
			var oldOwner = _owner;
			_owner = value;
			OnOwnerChanged( oldOwner, value );
		}
	}
	/// <summary>
	/// See <see cref="Owner"/>.
	/// </summary>
	private INetworkClient? _owner;

	/// <summary>
	/// The world position of the <see cref="NetworkEntity"/>.
	/// </summary>
	[ClientAuthority, Lerp]
	public NetworkedVector3 Position { get; set; }

	/// <summary>
	/// The world rotation of the <see cref="NetworkEntity"/>.
	/// </summary>
	[ClientAuthority, Lerp]
	public NetworkedQuaternion Rotation { get; set; }

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkEntity"/>.
	/// </summary>
	public NetworkEntity()
	{
		IEntity.AllEntities.Add( this );
	}

	/// <summary>
	/// Updates the entity.
	/// </summary>
	public virtual void Update()
	{
		if ( Realm.IsClient )
			UpdateClient();
		if ( Realm.IsServer )
			UpdateServer();
	}

	/// <summary>
	/// Called when ownership of the entity has changed.
	/// </summary>
	/// <param name="oldOwner">The old owner of the entity.</param>
	/// <param name="newOwner">The new owner of the entity.</param>
	protected virtual void OnOwnerChanged( INetworkClient? oldOwner, INetworkClient? newOwner )
	{
	}

	/// <summary>
	/// Deletes the <see cref="NetworkEntity"/>. You should not be using this after it is invoked.
	/// </summary>
	public override void Delete()
	{
		base.Delete();

		IEntity.AllEntities.Remove( this );
	}

	/// <summary>
	/// Deserializes all changes relating to the <see cref="NetworkEntity"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public sealed override void DeserializeChanges( NetworkReader reader )
	{
		var changedCount = reader.ReadInt32();
		for ( var i = 0; i < changedCount; i++ )
		{
			var propertyName = reader.ReadString();
			var property = PropertyNameCache[propertyName];

			if ( Realm.IsClient && Owner == INetworkClient.Local && property.HasAttribute<ClientAuthorityAttribute>() )
			{
				// TODO: What a cunt of a workaround
				IGlue.Instance.TypeLibrary.Create<INetworkable>( property.PropertyType )!.DeserializeChanges( reader );
				continue;
			}

			if ( property.PropertyType.IsAssignableTo( typeof( BaseNetworkable ) ) )
			{
				var networkId = reader.ReadInt32();
				var networkable = All.FirstOrDefault( networkable => networkable.NetworkId == networkId );
				if ( networkable is not null )
					property.SetValue( this, networkable );
				else if ( Realm.IsClient )
					PendingNetworkables.Add( networkId, propertyName );
			}
			else
			{
				if ( Realm.IsClient && property.HasAttribute<LerpAttribute>() )
				{
					var oldValue = property.GetValue( this ) as INetworkable;
					var newValue = property.GetValue( this ) as INetworkable;
					newValue!.DeserializeChanges( reader );

					LerpBucket[propertyName] = (oldValue, newValue);
				}
				else
				{
					var currentValue = property.GetValue( this );
					(currentValue as INetworkable)!.DeserializeChanges( reader );
					if ( IGlue.Instance.TypeLibrary.IsStruct( property.PropertyType ) )
						property.SetValue( this, currentValue );
				}
			}
		}
	}

	/// <summary>
	/// Returns a string that represents the <see cref="NetworkEntity"/>.
	/// </summary>
	/// <returns>A string that represents the <see cref="NetworkEntity"/>.</returns>
	public override string ToString()
	{
		return $"Entity (ID: {EntityId})";
	}
}
