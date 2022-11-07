using System;

namespace NetBolt.Shared.Utility;

/// <summary>
/// An abstraction over a typical <see cref="System.Reflection.PropertyInfo"/>.
/// </summary>
public interface IProperty
{
	/// <summary>
	/// Whether or not the property is static.
	/// </summary>
	bool IsStatic { get; }
	/// <summary>
	/// The name of the property.
	/// </summary>
	string Name { get; }
	/// <summary>
	/// The underlying type of the property.
	/// </summary>
	Type PropertyType { get; }

	/// <summary>
	/// Returns whether or not the property has an attribute.
	/// </summary>
	/// <typeparam name="T">The type of the attribute to look for.</typeparam>
	/// <returns>Whether or not the property has the attribute.</returns>
	bool HasAttribute<T>() where T : Attribute;

	/// <summary>
	/// Gets the value of the property.
	/// </summary>
	/// <param name="instance">The instance to get the value from.</param>
	/// <returns>The value of the property.</returns>
	object? GetValue( object? instance );
	/// <summary>
	/// Sets the value of the property.
	/// </summary>
	/// <param name="instance">The instance to set the value on.</param>
	/// <param name="obj">The value to set on the property.</param>
	void SetValue( object? instance, object? obj );
}
