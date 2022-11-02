﻿using NetBolt.Shared;
using NetBolt.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetBolt.Server.Glue;

/// <summary>
/// The glue required for reflection actions in NetBolt.
/// </summary>
public class TypeGlue : ITypeLibrary
{
	/// <summary>
	/// The singleton instance of <see cref="TypeGlue"/>.
	/// </summary>
	public static TypeGlue Instance = null!;

	/// <summary>
	/// The assembly to search for types.
	/// </summary>
	private readonly List<Assembly> Assemblies = new();
	/// <summary>
	/// A cache of type names mapped to their C# type.
	/// </summary>
	private readonly Dictionary<string, Type> TypeNameCache = new();

	/// <summary>
	/// Initializes a default instance of <see cref="TypeGlue"/>.
	/// </summary>
	internal TypeGlue()
	{
		Instance = this;
		AddAssembly( Assembly.GetExecutingAssembly() );

		var sharedAssembly = Assembly.GetAssembly( typeof( IGlue ) );
		if ( sharedAssembly is not null && sharedAssembly != Assembly.GetExecutingAssembly() )
			AddAssembly( sharedAssembly );
	}

	/// <summary>
	/// Adds an assembly to the type helper.
	/// </summary>
	/// <param name="assembly">The assembly to add.</param>
	public void AddAssembly( Assembly assembly )
	{
		Assemblies.Add( assembly );
	}

	/// <inheritdoc/>
	public T Create<T>()
	{
		return Activator.CreateInstance<T>();
	}

	/// <inheritdoc/>
	public T? Create<T>( Type typeToCreate )
	{
		return (T?)Activator.CreateInstance( typeToCreate );
	}

	/// <inheritdoc/>
	public T? Create<T>( params object[] parameters )
	{
		return (T?)Activator.CreateInstance( typeof( T ), parameters );
	}

	/// <inheritdoc/>
	public T? Create<T>( Type typeToCreate, params object?[] parameters )
	{
		return (T?)Activator.CreateInstance( typeToCreate, parameters );
	}

	/// <inheritdoc/>
	public T? Create<T>( Type baseTypeToCreate, Type[] genericTypes )
	{
		return (T?)Activator.CreateInstance( baseTypeToCreate.MakeGenericType( genericTypes ) );
	}

	/// <inheritdoc/>
	public IEnumerable<IProperty> GetAllProperties( Type type )
	{
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
		foreach ( var property in properties )
			yield return new PropertyInfoWrapper( property );
	}

	/// <inheritdoc/>
	public IMethod? GetMethodByName( Type type, string methodName )
	{
		var method = type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
		return method is not null ? new MethodInfoWrapper( method ) : null;
	}

	/// <inheritdoc/>
	public Type[] GetGenericArguments( Type type )
	{
		return type.GetGenericArguments();
	}

	/// <inheritdoc/>
	public Type? GetTypeByName( string name )
	{
		if ( TypeNameCache.TryGetValue( name, out var cachedType ) )
			return cachedType;

		foreach ( var assembly in Assemblies )
		{
			foreach ( var type in assembly.DefinedTypes )
			{
				if ( type.Name != name )
					continue;

				TypeNameCache.Add( name, type );
				return type;
			}
		}

		return null;
	}

	/// <inheritdoc/>
	public bool IsClass( Type type )
	{
		return type.IsClass;
	}

	/// <inheritdoc/>
	public bool IsStruct( Type type )
	{
		return type.IsValueType && !type.IsEnum;
	}
}
