using NetBolt.Shared.Utility;
using Sandbox;
using System;

namespace NetBolt.Client;

/// <summary>
/// A wrapper for <see cref="PropertyDescription"/> that works with NetBolt.
/// </summary>
internal class PropertyDescriptionWrapper : IProperty
{
	/// <inheritdoc/>
	public bool IsStatic => _propertyDescription.IsStatic;

	/// <inheritdoc/>
	public string Name => _propertyDescription.Name;

	/// <inheritdoc/>
	public Type PropertyType => _propertyDescription.PropertyType;

	/// <summary>
	/// The underlying <see cref="PropertyDescription"/>.
	/// </summary>
	private readonly PropertyDescription _propertyDescription;

	/// <summary>
	/// Initializes a new instance of <see cref="PropertyDescriptionWrapper"/> with an underlying <see cref="PropertyDescription"/>.
	/// </summary>
	/// <param name="propertyDescription">The underlying <see cref="PropertyDescription"/>.</param>
	internal PropertyDescriptionWrapper( PropertyDescription propertyDescription )
	{
		_propertyDescription = propertyDescription;
	}

	/// <inheritdoc/>
	public bool HasAttribute<T>() where T : Attribute
	{
		// TODO: Cache this
		return _propertyDescription.GetCustomAttribute<T>() is not null;
	}

	/// <inheritdoc/>
	public object? GetValue( object? instance )
	{
		return _propertyDescription.GetValue( instance );
	}

	/// <inheritdoc/>
	public void SetValue( object? instance, object? obj )
	{
		_propertyDescription.SetValue( instance, obj );
	}
}
