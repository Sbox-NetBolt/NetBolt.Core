using NetBolt.Server;
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
	[ClientAuthority]
	public NetworkedVector3 Position { get; set; }

	/// <summary>
	/// The world rotation of the <see cref="NetworkEntity"/>.
	/// </summary>
	[ClientAuthority]
	public NetworkedQuaternion Rotation { get; set; }

	/// <summary>
	/// Updates the entity.
	/// </summary>
	public virtual void Update()
	{
#if SERVER
		UpdateServer();
#endif
#if CLIENT
		UpdateClient();
#endif
	}

	/// <summary>
	/// Called when ownership of the entity has changed.
	/// </summary>
	/// <param name="oldOwner">The old owner of the entity.</param>
	/// <param name="newOwner">The new owner of the entity.</param>
	protected virtual void OnOwnerChanged( INetworkClient? oldOwner, INetworkClient? newOwner )
	{
	}

	internal override void Delete()
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
			var property = PropertyNameCache[reader.ReadString()];
#if CLIENT
			if ( Owner == INetworkClient.Local && property.GetCustomAttribute<ClientAuthorityAttribute>() is not null )
			{
				// TODO: What a cunt of a workaround
				TypeHelper.Create<INetworkable>( property.PropertyType )!.DeserializeChanges( reader );
				continue;
			}
#endif

			var currentValue = property.GetValue( this );
			(currentValue as INetworkable)!.DeserializeChanges( reader );
			property.SetValue( this, currentValue );
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
