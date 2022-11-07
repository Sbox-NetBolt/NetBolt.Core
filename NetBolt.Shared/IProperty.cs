using NetBolt.Shared.Networkables;
using System;

namespace NetBolt.Shared;

/// <summary>
/// An abstraction over a typical <see cref="System.Reflection.PropertyInfo"/>.
/// </summary>
public interface IProperty
{
	/// <summary>
	/// Whether or not the property is networkable.
	/// </summary>
	bool IsNetworkable { get; }
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

	/// <summary>
	/// The default implementation to determine whether or not a property is networkable.
	/// </summary>
	/// <param name="property">The property to check if it is networkable.</param>
	/// <returns>Whether or not the property is networkable.</returns>
	public static bool DefaultIsNetworkable( IProperty property )
	{
		if ( !property.PropertyType.IsAssignableTo( typeof( ComplexNetworkable ) ) )
			return false;

		if ( property.HasAttribute<NoNetworkAttribute>() || property.IsStatic )
			return false;

		return true;
	}
}
