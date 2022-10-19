using System;

namespace NetBolt.Shared.Utility;

/// <summary>
/// An abstraction over a typical <see cref="System.Reflection.MethodInfo"/>.
/// </summary>
public interface IMethod
{
	/// <summary>
	/// Returns whether or not the method has an attribute.
	/// </summary>
	/// <typeparam name="T">The type of the attribute to look for.</typeparam>
	/// <returns>Whether or not the method has the attribute.</returns>
	public bool HasAttribute<T>() where T : Attribute;

	/// <summary>
	/// Invokes a method and returns a value.
	/// </summary>
	/// <param name="instance">The instance to pass to the invoke.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	/// <returns>The returned value from the method.</returns>
	public object? Invoke( object? instance, params object?[] parameters );
}
