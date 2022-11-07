using NetBolt.Shared;
using System;
using System.Reflection;

namespace NetBolt.Server.Glue;

/// <summary>
/// A wrapper for <see cref="MethodInfo"/> that works with NetBolt.
/// </summary>
internal class MethodInfoWrapper : IMethod
{
	/// <summary>
	/// The underlying <see cref="MethodInfo"/>.
	/// </summary>
	private readonly MethodInfo _methodInfo;

	/// <summary>
	/// Initializes a new instance of <see cref="MethodInfoWrapper"/> with an underlying <see cref="MethodInfo"/>.
	/// </summary>
	/// <param name="methodInfo">The underlying <see cref="MethodInfo"/>.</param>
	internal MethodInfoWrapper( MethodInfo methodInfo )
	{
		_methodInfo = methodInfo;
	}

	/// <inheritdoc/>
	public bool HasAttribute<T>() where T : Attribute
	{
		// TODO: Cache this
		return _methodInfo.GetCustomAttribute<T>() is not null;
	}

	/// <inheritdoc/>
	public object? Invoke( object? instance, object?[] parameters )
	{
		return _methodInfo.Invoke( instance, parameters );
	}
}
