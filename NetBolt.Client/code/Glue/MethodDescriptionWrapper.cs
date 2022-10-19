using NetBolt.Shared.Utility;
using Sandbox;
using System;

namespace NetBolt.Client;

/// <summary>
/// A wrapper for <see cref="MethodDescription"/> that works with NetBolt.
/// </summary>
internal class MethodDescriptionWrapper : IMethod
{
	/// <summary>
	/// The underlying <see cref="MethodDescription"/>.
	/// </summary>
	private readonly MethodDescription _methodDescription;

	/// <summary>
	/// Initializes a new instance of <see cref="MethodDescriptionWrapper"/> with an underlying <see cref="MethodDescription"/>.
	/// </summary>
	/// <param name="methodDescription">The underlying <see cref="MethodDescription"/>.</param>
	internal MethodDescriptionWrapper( MethodDescription methodDescription )
	{
		_methodDescription = methodDescription;
	}

	/// <inheritdoc/>
	public bool HasAttribute<T>() where T : Attribute
	{
		// TODO: Cache this
		return _methodDescription.GetCustomAttribute<T>() is not null;
	}

	/// <inheritdoc/>
	public object? Invoke( object? instance, object?[] parameters )
	{
		return _methodDescription.InvokeWithReturn<object?>( instance, parameters );
	}
}
