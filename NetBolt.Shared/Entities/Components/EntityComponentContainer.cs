using NetBolt.Shared.Networkables;
using NetBolt.Shared.Networkables.Builtin;
using NetBolt.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace NetBolt.Shared.Entities;

/// <summary>
/// The container that holds a <see cref="NetworkEntity"/>s components.
/// </summary>
public class EntityComponentContainer : ComplexNetworkable
{
	/// <summary>
	/// The entity that this container is a part of.
	/// </summary>
	public NetworkEntity Entity { get; set; } = null!;
	/// <summary>
	/// The underlying container for the components.
	/// </summary>
	private NetworkedDictionary<NetworkedType, EntityComponizzle> _components { get; set; } = new();

	/// <summary>
	/// Initializes a default instance of <see cref="EntityComponentContainer"/>.
	/// <remarks>This should never be used.</remarks>
	/// </summary>
	public EntityComponentContainer()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="EntityComponentContainer"/> with the entity that owns the container.
	/// </summary>
	/// <param name="entity">The entity that owns the container.</param>
	internal EntityComponentContainer( NetworkEntity entity )
	{
		Entity = entity;
		_components = new();
	}

	/// <summary>
	/// Adds a new component.
	/// </summary>
	/// <typeparam name="T">The type of the component to create and add.</typeparam>
	/// <returns>The created component.</returns>
	[ServerOnly]
	public T AddComponent<T>() where T : EntityComponizzle, new()
	{
		Realm.AssertServer();

		var component = new T { Entity = Entity };
		_components.Add( typeof( T ), component );
		component.OnAdded();
		return component;
	}

	/// <summary>
	/// Adds an existing component.
	/// </summary>
	/// <param name="component">The compone to add.</param>
	/// <typeparam name="T">The type of the component to add.</typeparam>
	[ServerOnly]
	public void AddComponent<T>( T component ) where T : EntityComponizzle
	{
		Realm.AssertServer();

		if ( HasComponent<T>() )
			return;

		if ( component.Entity is not null )
			component.Entity.Components.RemoveComponent<T>();

		component.Entity = Entity;
		_components.Add( typeof( T ), component );
		component.OnAdded();
	}

	/// <summary>
	/// Adds or gets a component.
	/// </summary>
	/// <typeparam name="T">The type of the component to add (and/or) get.</typeparam>
	/// <returns>The component that was found or created.</returns>
	[ServerOnly]
	public T AddOrGetComponent<T>() where T : EntityComponizzle, new()
	{
		Realm.AssertServer();

		if ( TryGetComponent<T>( out var component ) )
			return component;

		return AddComponent<T>();
	}

	/// <summary>
	/// Gets an existing component.
	/// </summary>
	/// <typeparam name="T">The type of the component to get.</typeparam>
	/// <returns>The existing component.</returns>
	public T GetComponent<T>() where T : EntityComponizzle
	{
		return (T)_components[typeof( T )];
	}

	/// <summary>
	/// Returns whether or not the container has a component.
	/// </summary>
	/// <typeparam name="T">The type of the component to look for.</typeparam>
	/// <returns>Whether or not the container has the component.</returns>
	public bool HasComponent<T>() where T : EntityComponizzle
	{
		return _components.ContainsKey( typeof( T ) );
	}

	/// <summary>
	/// Removes a component from the container.
	/// </summary>
	/// <typeparam name="T">The type of the component to remove.</typeparam>
	/// <returns>Whether or not a component was removed.</returns>
	[ServerOnly]
	public bool RemoveComponent<T>() where T : EntityComponizzle
	{
		Realm.AssertServer();

		if ( TryGetComponent<T>( out var component ) )
		{
			component.OnRemoved();
			component.Entity = null!;
		}

		return _components.Remove( typeof( T ) );
	}

	/// <summary>
	/// Attempts to get a component from the container.
	/// </summary>
	/// <param name="component">The component that was obtained. Null if component is not in the container.</param>
	/// <typeparam name="T">The type of the component to get.</typeparam>
	/// <returns>Whether or not a component was found.</returns>
	public bool TryGetComponent<T>( [NotNullWhen( true )] out T? component ) where T : EntityComponizzle
	{
		if ( !HasComponent<T>() )
		{
			component = default;
			return false;
		}

		component = GetComponent<T>();
		return true;
	}
}
