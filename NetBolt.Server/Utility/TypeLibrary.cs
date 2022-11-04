using NetBolt.Server.Glue;
using NetBolt.Shared;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetBolt.Server.Utility;

/// <summary>
/// The glue required for reflection actions in NetBolt.
/// </summary>
public class TypeLibrary : ITypeLibrary
{
	/// <summary>
	/// The singleton instance of <see cref="TypeLibrary"/>.
	/// </summary>
	public static TypeLibrary Instance = null!;

	/// <summary>
	/// The assembly to search for types.
	/// </summary>
	private readonly List<Assembly> Assemblies = new();
	/// <summary>
	/// A cache of type names mapped to their C# type.
	/// </summary>
	private readonly Dictionary<string, Type> TypeNameCache = new();
	/// <summary>
	/// A cache containing all types that derive from <see cref="INetworkable"/> with a unique number attached.
	/// </summary>
	private Dictionary<Type, ushort>? NetworkableTypeCache = null;

	/// <summary>
	/// Initializes a default instance of <see cref="TypeLibrary"/>.
	/// </summary>
	internal TypeLibrary()
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
	/// Gets the internal dictionary for cached networkable types.
	/// </summary>
	public IReadOnlyDictionary<Type, ushort> GetNetworkableDictionary()
	{
		if ( NetworkableTypeCache is null )
			CacheNetworkableTypes();

		return NetworkableTypeCache!;
	}

	/// <summary>
	/// Resets the <see cref="NetworkableTypeCache"/>. Used for when adding new assmeblies and needing to flush the cache.
	/// </summary>
	public void ResetNetworkableTypeCache()
	{
		NetworkableTypeCache = null!;
	}

	/// <summary>
	/// Caches the <see cref="NetworkableTypeCache"/>.
	/// </summary>
	internal void CacheNetworkableTypes()
	{
		foreach ( var _ in GetAllNetworkableTypes() )
		{
		}
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
	public IEnumerable<Type> GetAllNetworkableTypes()
	{
		if ( NetworkableTypeCache is not null )
		{
			foreach ( var (type, _) in NetworkableTypeCache )
				yield return type;
		}

		NetworkableTypeCache = new();
		ushort i = 1;
		foreach ( var assembly in Assemblies )
		{
			foreach ( var type in assembly.DefinedTypes )
			{
				if ( !type.IsAssignableTo( typeof( INetworkable ) ) )
					continue;

				if ( i == 0 )
					Log.Error( "Ran out of networkable type indices" );

				NetworkableTypeCache.Add( type, i++ );
				yield return type;
			}
		}
	}

	/// <inheritdoc/>
	public IEnumerable<IProperty> GetAllProperties( Type type )
	{
		var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
		foreach ( var property in properties )
			yield return new PropertyInfoWrapper( property );
	}

	/// <inheritdoc/>
	public Type[] GetGenericArguments( Type type )
	{
		return type.GetGenericArguments();
	}

	/// <inheritdoc/>
	public ushort GetIdentifierFromNetworkableType( Type type )
	{
		if ( !type.IsAssignableTo( typeof( INetworkable ) ) )
		{
			Log.Error( $"{type} does not implement {typeof( INetworkable )}" );
			return 0;
		}

		if ( NetworkableTypeCache is null )
			CacheNetworkableTypes();

		if ( type.IsConstructedGenericType )
			return NetworkableTypeCache![type.GetGenericTypeDefinition()];

		return NetworkableTypeCache![type];
	}

	/// <inheritdoc/>
	public IMethod? GetMethodByName( Type type, string methodName )
	{
		var method = type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
		return method is not null ? new MethodInfoWrapper( method ) : null;
	}

	/// <inheritdoc/>
	public Type? GetNetworkableTypeByIdentifier( ushort identifier )
	{
		if ( NetworkableTypeCache is null )
			CacheNetworkableTypes();

		foreach ( var (type, id) in NetworkableTypeCache! )
		{
			if ( id == identifier )
				return type;
		}

		return null;
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
