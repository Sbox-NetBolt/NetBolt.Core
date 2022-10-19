using NetBolt.Shared.Utility;
using System;
using System.Reflection;

namespace NetBolt.Server.Glue;

/// <summary>
/// A wrapper for <see cref="PropertyInfo"/> that works with NetBolt.
/// </summary>
internal class PropertyInfoWrapper : IProperty
{
	/// <inheritdoc/>
	public string Name => _propertyInfo.Name;

	/// <inheritdoc/>
	public Type PropertyType => _propertyInfo.PropertyType;

	/// <summary>
	/// The underlying <see cref="PropertyInfo"/>.
	/// </summary>
	private readonly PropertyInfo _propertyInfo;

	/// <summary>
	/// Initializes a new instance of <see cref="PropertyInfoWrapper"/> with an underlyinf <see cref="PropertyInfo"/>.
	/// </summary>
	/// <param name="propertyInfo">The underlying <see cref="PropertyInfo"/>.</param>
	internal PropertyInfoWrapper( PropertyInfo propertyInfo )
	{
		_propertyInfo = propertyInfo;
	}

	/// <inheritdoc/>
	public bool HasAttribute<T>() where T : Attribute
	{
		// TODO: Cache this
		return _propertyInfo.GetCustomAttribute<T>() is not null;
	}

	/// <inheritdoc/>
	public object? GetValue( object? instance )
	{
		return _propertyInfo.GetValue( instance );
	}

	/// <inheritdoc/>
	public void SetValue( object? instance, object? obj )
	{
		_propertyInfo.SetValue( instance, obj );
	}
}
