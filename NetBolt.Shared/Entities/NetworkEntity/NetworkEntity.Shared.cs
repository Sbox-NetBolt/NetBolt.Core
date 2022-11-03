using NetBolt.Shared.Clients;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Networkables.Builtin;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Entities;

/// <summary>
/// The base class for all of your entity needs.
/// </summary>
public partial class NetworkEntity : ComplexNetworkable, IEntity
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
	/// A container for components on the <see cref="NetworkEntity"/>.
	/// </summary>
	public EntityComponentContainer Components { get; set; }

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkEntity"/>.
	/// </summary>
	public NetworkEntity()
	{
		Components = new EntityComponentContainer( this );
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
	public override void DeserializeChanges( NetworkReader reader )
	{
		var changedCount = reader.ReadInt32();
		for ( var i = 0; i < changedCount; i++ )
		{
			var propertyName = reader.ReadString();
			var property = PropertyNameCache[propertyName];

			if ( Realm.IsClient && Owner == INetworkClient.Local && property.HasAttribute<ClientAuthorityAttribute>() )
			{
				// TODO: What a cunt of a workaround
				ITypeLibrary.Instance.Create<INetworkable>( property.PropertyType )!.DeserializeChanges( reader );
				continue;
			}

			if ( property.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) )
			{
				var networkId = reader.ReadInt32();
				INetworkable? networkable;

				if ( Realm.IsClient )
				{
					networkable = GetOrRequestById( networkId, complexNetworkable =>
					{
						property.SetValue( this, complexNetworkable );
						_referenceBucket[propertyName] = complexNetworkable;
					} );
				}
				else
					networkable = GetById( networkId );

				property.SetValue( this, networkable );
				_referenceBucket[propertyName] = networkable;
			}
			else if ( Realm.IsClient && property.HasAttribute<LerpAttribute>() )
			{
				var oldValue = (INetworkable)property.GetValue( this )!;
				var newValue = (INetworkable)property.GetValue( this )!;
				newValue.DeserializeChanges( reader );

				LerpBucket[propertyName] = (oldValue, newValue);
				_referenceBucket[propertyName] = newValue;
			}
			else
			{
				if ( property.GetValue( this ) is not INetworkable currentValue )
				{
					ILogger.Instance.Error( "{0} is missing its value on {1}", this, propertyName );
					continue;
				}

				currentValue.DeserializeChanges( reader );
				if ( ITypeLibrary.Instance.IsStruct( property.PropertyType ) )
					property.SetValue( this, currentValue );

				_referenceBucket[propertyName] = currentValue;
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
