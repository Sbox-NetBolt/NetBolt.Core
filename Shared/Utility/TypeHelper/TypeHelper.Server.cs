#if SERVER
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetBolt.Shared.Utility;

public static partial class TypeHelper
{
	/// <summary>
	/// The assembly to search for types.
	/// </summary>
	private static readonly List<Assembly> Assemblies = new() { Assembly.GetExecutingAssembly() };
	/// <summary>
	/// A cache of type names mapped to their C# type.
	/// </summary>
	private static readonly Dictionary<string, Type> TypeNameCache = new();

	/// <summary>
	/// Adds an assembly to the type helper.
	/// </summary>
	/// <param name="assembly">The assembly to add.</param>
	public static void AddAssembly( Assembly assembly )
	{
		Assemblies.Add( assembly );
	}

	/// <summary>
	/// Creates an instance of <see ref="T"/>.
	/// </summary>
	/// <typeparam name="T">The type to create an instance of.</typeparam>
	/// <returns>The created instance of <see ref="T"/>.</returns>
	public static T Create<T>()
	{
		return Activator.CreateInstance<T>();
	}

	/// <summary>
	/// Creates an instance of <see ref="T"/>.
	/// </summary>
	/// <param name="parameters">The parameters to pass to the public constructor.</param>
	/// <typeparam name="T">The type to create an instance of.</typeparam>
	/// <returns>The created instance of <see ref="T"/>.</returns>
	public static T? Create<T>( params object[] parameters )
	{
		return (T?)Activator.CreateInstance( typeof( T ), parameters );
	}

	/// <summary>
	/// Creates an instance of <see ref="typeToCreate"/> and casts it to <see ref="T"/>.
	/// </summary>
	/// <param name="typeToCreate">The type to create.</param>
	/// <param name="parameters">The parameters to pass to the public constructor.</param>
	/// <typeparam name="T">The type to cast the created instance to.</typeparam>
	/// <returns>The created instance of <see ref="typeToCreate"/> casted to <see ref="T"/>.</returns>
	public static T? Create<T>( Type typeToCreate, params object[] parameters )
	{
		return (T?)Activator.CreateInstance( typeToCreate, parameters );
	}

	/// <summary>
	/// Creates an instance of <see ref="baseTypeToCreate"/> with <see ref="genericTypes"/> generics and casted to <see ref="T"/>.
	/// </summary>
	/// <param name="baseTypeToCreate">The base type to create.</param>
	/// <param name="genericTypes">The generic arguments of <see ref="baseTypeToCreate"/>.</param>
	/// <typeparam name="T">The type to cast the created instance to.</typeparam>
	/// <returns>The created instance casted to <see ref="T"/>.</returns>
	public static T? Create<T>( Type baseTypeToCreate, Type[] genericTypes )
	{
		return (T?)Activator.CreateInstance( baseTypeToCreate.MakeGenericType( genericTypes ) );
	}

	/// <summary>
	/// Gets the generic arguments of a type.
	/// </summary>
	/// <param name="type">The type to get the generic arguments of.</param>
	/// <returns>The generic arguments on the type.</returns>
	public static Type[] GetGenericArguments( Type type )
	{
		return type.GetGenericArguments();
	}

	/// <summary>
	/// Gets all properties on the type.
	/// </summary>
	/// <param name="type">The type to get the properties of.</param>
	/// <returns>The properties on the type.</returns>
	public static PropertyInfo[] GetAllProperties( Type type )
	{
		return type.GetProperties( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
	}

	/// <summary>
	/// Gets a C# type by its name.
	/// </summary>
	/// <param name="name">The name of the type.</param>
	/// <returns>The type that was found. Null if none were found.</returns>
	public static Type? GetTypeByName( string name )
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

	/// <summary>
	/// Returns whether or not a type is a class.
	/// </summary>
	/// <param name="type">The type to check if it is a class.</param>
	/// <returns>Whether or not the type is a class.</returns>
	public static bool IsClass( Type type )
	{
		return type.IsClass;
	}

	/// <summary>
	/// Returns whether or not a type is a struct.
	/// </summary>
	/// <param name="type">The type to check if it is a struct.</param>
	/// <returns>Whether or not a type is a struct.</returns>
	public static bool IsStruct( Type type )
	{
		return type.IsValueType && !type.IsEnum;
	}
}
#endif
