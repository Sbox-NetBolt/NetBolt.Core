using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;
using System;
using System.Collections.Generic;

namespace NetBolt.Shared;

/// <summary>
/// The glue required for Reflection actions.
/// </summary>
public interface ITypeLibrary
{
	/// <summary>
	/// Creates an instance of <see ref="type"/> and casts it to <see ref="T"/>.
	/// </summary>
	/// <param name="type">The type to create.</param>
	/// <typeparam name="T">The type to cast the instance to.</typeparam>
	/// <returns>The created instance of <see ref="type"/> casted to <see ref="T"/>. Null if something failed at any point.</returns>
	T? Create<T>( Type type );
	/// <summary>
	/// Creates an instance of <see ref="type"/> with <see ref="genericTypes"/> and casts it to <see ref="T"/>.
	/// </summary>
	/// <param name="type">The base type to create.</param>
	/// <param name="genericTypes">The generic arguments of the type to create.</param>
	/// <typeparam name="T">The type to cast the instance to.</typeparam>
	/// <returns>The created instance of <see ref="type"/> casted to <see ref="T"/>. Null if something failed at any point.</returns>
	T? Create<T>( Type type, Type[] genericTypes );

	/// <summary>
	/// Gets all networkable types in any assemblies that could be used during runtime.
	/// </summary>
	/// <returns>All networkable types in any assemblies that could be used during runtime.</returns>
	IEnumerable<Type> GetAllNetworkableTypes();
	/// <summary>
	/// Gets all properties on a type.
	/// </summary>
	/// <param name="type">The type to get all properties from.</param>
	/// <returns>All properties on the type.</returns>
	IEnumerable<IProperty> GetAllProperties( Type type );
	/// <summary>
	/// Gets the generic arguments on a type.
	/// </summary>
	/// <param name="type">The type to get the generic arguments from.</param>
	/// <returns>The generic arguments on the type.</returns>
	Type[] GetGenericArguments( Type type );
	/// <summary>
	/// Gets the cache identifier for the provided <see cref="INetworkable"/> derived type.
	/// </summary>
	/// <param name="type">The type to get the identifier of.</param>
	/// <returns>The identifier of the type. 0 if invalid.</returns>
	ushort GetIdentifierFromNetworkableType( Type type );
	/// <summary>
	/// Gets a method by its name and type it is a member of.
	/// </summary>
	/// <param name="type">The type that the method is a member of.</param>
	/// <param name="methodName">The name of the method to get.</param>
	/// <returns>The found method. Null otherwise.</returns>
	IMethod? GetMethodByName( Type type, string methodName );
	/// <summary>
	/// Gets a <see cref="INetworkable"/> derived type by its cache identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the type.</param>
	/// <returns>The derived type if found. Null otherwise.</returns>
	Type? GetNetworkableTypeByIdentifier( ushort identifier );
	/// <summary>
	/// Gets a C# type by its type name.
	/// </summary>
	/// <param name="typeName">The name of the type to get.</param>
	/// <returns>The C# type if found. Null otherwise.</returns>
	Type? GetTypeByName( string typeName );

	/// <summary>
	/// Returns whether or not a type is a class.
	/// </summary>
	/// <param name="type">The type to check if it is a class.</param>
	/// <returns>Whether or not the type is a class.</returns>
	bool IsClass( Type type );
	/// <summary>
	/// Returns whether or not a type is a struct.
	/// </summary>
	/// <param name="type">The type to check if it is a struct.</param>
	/// <returns>Whether or not the type is a struct.</returns>
	bool IsStruct( Type type );

	/// <summary>
	/// The instance to access for type library glue functionality.
	/// </summary>
	public static ITypeLibrary Instance { get; internal set; } = null!;
}
