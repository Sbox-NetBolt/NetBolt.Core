using NetBolt.Shared;
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

	/// <summary>
	/// Creates an instance of <see ref="T"/>.
	/// </summary>
	/// <typeparam name="T">The type to create an instance of.</typeparam>
	/// <returns>The created instance of <see ref="T"/>.</returns>
	public T Create<T>()
	{
		return Activator.CreateInstance<T>();
	}

	/// <summary>
	/// Creates an instance of <see ref="typeToCreate"/> and casts it to <see ref="T"/>.
	/// </summary>
	/// <param name="typeToCreate">The type to create.</param>
	/// <typeparam name="T">The type to cast the created instance to.</typeparam>
	/// <returns>The created isntance of <see ref="typeToCreate"/> casted to <see ref="T"/>.</returns>
	public T? Create<T>( Type typeToCreate )
	{
		return (T?)Activator.CreateInstance( typeToCreate );
	}

	/// <summary>
	/// Creates an instance of <see ref="T"/>.
	/// </summary>
	/// <param name="parameters">The parameters to pass to the public constructor.</param>
	/// <typeparam name="T">The type to create an instance of.</typeparam>
	/// <returns>The created instance of <see ref="T"/>.</returns>
	public T? Create<T>( params object[] parameters )
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
	public T? Create<T>( Type typeToCreate, params object?[] parameters )
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
	public T? Create<T>( Type baseTypeToCreate, Type[] genericTypes )
	{
		return (T?)Activator.CreateInstance( baseTypeToCreate.MakeGenericType( genericTypes ) );
	}

	/// <summary>
	/// Gets all properties on the type.
	/// </summary>
	/// <param name="type">The type to get the properties of.</param>
	/// <returns>The properties on the type.</returns>
	public IEnumerable<IProperty> GetAllProperties( Type type )
	{
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
		foreach ( var property in properties )
			yield return new PropertyInfoWrapper( property );
	}

	/// <summary>
	/// Gets a method on a type.
	/// </summary>
	/// <param name="type">The type to search in for the method.</param>
	/// <param name="methodName">The name of the method to search for.</param>
	/// <returns>The method if found. Null otherwise.</returns>
	public IMethod? GetMethodByName( Type type, string methodName )
	{
		var method = type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
		return method is not null ? new MethodInfoWrapper( method ) : null;
	}

	/// <summary>
	/// Gets the generic arguments of a type.
	/// </summary>
	/// <param name="type">The type to get the generic arguments of.</param>
	/// <returns>The generic arguments on the type.</returns>
	public Type[] GetGenericArguments( Type type )
	{
		return type.GetGenericArguments();
	}

	/// <summary>
	/// Gets a C# type by its name.
	/// </summary>
	/// <param name="name">The name of the type.</param>
	/// <returns>The type that was found. Null if none were found.</returns>
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

	/// <summary>
	/// Returns whether or not a type is a class.
	/// </summary>
	/// <param name="type">The type to check if it is a class.</param>
	/// <returns>Whether or not the type is a class.</returns>
	public bool IsClass( Type type )
	{
		return type.IsClass;
	}

	/// <summary>
	/// Returns whether or not a type is a struct.
	/// </summary>
	/// <param name="type">The type to check if it is a struct.</param>
	/// <returns>Whether or not a type is a struct.</returns>
	public bool IsStruct( Type type )
	{
		return type.IsValueType && !type.IsEnum;
	}
}
