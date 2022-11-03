using System;
using System.IO;
using System.Numerics;
using NetBolt.Shared.Networkables;

namespace NetBolt.Shared.Utility;

/// <summary>
/// Reads any data relating to networking and primitive types.
/// </summary>
public sealed class NetworkReader : BinaryReader
{
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkReader"/> with the <see cref="Stream"/> to read from.
	/// </summary>
	/// <param name="input">The underlying <see cref="Stream"/> to read from.</param>
	public NetworkReader( Stream input ) : base( input )
	{
	}

	/// <summary>
	/// Reads a 16 byte Globally Unique Identifier (<see cref="Guid"/>).
	/// </summary>
	/// <returns>The parsed <see cref="Guid"/>.</returns>
	public Guid ReadGuid()
	{
		return new Guid( ReadBytes( 16 ) );
	}

	/// <summary>
	/// Reads a 4 float <see cref="Quaternion"/>.
	/// </summary>
	/// <returns>The parsed <see cref="Quaternion"/>.</returns>
	public Quaternion ReadQuaternion()
	{
		return new Quaternion( ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle() );
	}

	/// <summary>
	/// Reads a 3 float <see cref="System.Numerics.Vector3"/>.
	/// </summary>
	/// <returns>The parsed <see cref="System.Numerics.Vector3"/>.</returns>
	public System.Numerics.Vector3 ReadVector3()
	{
		return new Vector3( ReadSingle(), ReadSingle(), ReadSingle() );
	}

	/// <summary>
	/// Reads a type.
	/// </summary>
	/// <param name="genericArguments">The generic arguments that were alongside the base type.</param>
	/// <returns>The base type.</returns>
	public Type ReadType( out Type[] genericArguments )
	{
		Type? type;
		if ( ReadBoolean() )
		{
			var typeIdentifier = ReadUInt16();
			type = ITypeLibrary.Instance.GetNetworkableTypeByIdentifier( typeIdentifier );

			if ( type is null )
			{
				ILogger.Instance.Error( "Failed to read type (type with identifier {0} was not found)", typeIdentifier );
				genericArguments = null!;
				return null!;
			}
		}
		else
		{
			var typeName = ReadString();
			type = ITypeLibrary.Instance.GetTypeByName( typeName );

			if ( type is null )
			{
				ILogger.Instance.Error( "Failed to read type (type with name \"{0}\" was not found)", typeName );
				genericArguments = null!;
					return null!;
			}
		}

		if ( type.IsGenericType )
		{
			var genericCount = ReadInt32();
			genericArguments = new Type[genericCount];
			for ( var i = 0; i < genericCount; i++ )
			{
				Type? genericType;
				if ( ReadBoolean() )
				{
					var genericTypeIdentifier = ReadUInt16();
					genericType = ITypeLibrary.Instance.GetNetworkableTypeByIdentifier( genericTypeIdentifier );

					if ( genericType is null )
					{
						ILogger.Instance.Error( "Failed to read type (generic argument #{0} with identifier {1} was not found)", i + 1, genericTypeIdentifier );
						genericArguments = null!;
						return null!;
					}
				}
				else
				{
					var genericTypeName = ReadString();
					genericType = ITypeLibrary.Instance.GetTypeByName( genericTypeName );

					if ( genericType is null )
					{
						ILogger.Instance.Error( "Failed to read type (generic argument #{0} with name \"{1}\" was not found)", i + 1, genericTypeName );
						genericArguments = null!;
						return null!;
					}
				}

				genericArguments[i] = genericType;
			}

			return type;
		}

		genericArguments = Array.Empty<Type>();
		return type;
	}

	/// <summary>
	/// Reads an instance of <see cref="INetworkable"/>.
	/// </summary>
	/// <returns>The parsed <see cref="INetworkable"/>.</returns>
	/// <exception cref="InvalidOperationException">Thrown when reading the <see cref="INetworkable"/> has failed.</exception>
	public INetworkable ReadNetworkable()
	{
		var type = ReadType( out var genericArguments );

		INetworkable? networkable;
		if ( genericArguments.Length > 0 )
			networkable = ITypeLibrary.Instance.Create<INetworkable>( type, genericArguments );
		else
			networkable = ITypeLibrary.Instance.Create<INetworkable>( type );

		if ( networkable is null )
		{
			ILogger.Instance.Error( "Failed to read networkable (instance creation failed)." );
			return null!;
		}

		networkable.Deserialize( this );
		return networkable;
	}

	/// <summary>
	/// Reads an instance of <see cref="INetworkable"/> and casts it to <see ref="T"/>.
	/// </summary>
	/// <typeparam name="T">The <see cref="INetworkable"/> type to cast into.</typeparam>
	/// <returns>The parsed <see cref="INetworkable"/>.</returns>
	/// <exception cref="InvalidOperationException">Thrown when reading the <see cref="INetworkable"/> has failed.</exception>
	public T ReadNetworkable<T>() where T : INetworkable
	{
		if ( typeof( T ).IsAssignableTo( typeof( ComplexNetworkable ) ) )
			return ReadComplexNetworkable<T>();

		var networkable = ReadNetworkable();
		if ( networkable is not T outputNetworkable )
		{
			ILogger.Instance.Error( "Failed to read networkable ({0} is not assignable to {1}).", networkable.GetType(), typeof( T ) );
			return default!;
		}

		return outputNetworkable;
	}

	/// <summary>
	/// Reads all changes relating to an <see cref="INetworkable"/> instance.
	/// </summary>
	/// <param name="networkable">The <see cref="INetworkable"/> to deserialize the changes into.</param>
	public void ReadNetworkableChanges( INetworkable networkable )
	{
		networkable.DeserializeChanges( this );
	}

	/// <summary>
	/// Reads an instance of <see cref="ComplexNetworkable"/>.
	/// </summary>
	/// <returns>The parsed <see cref="ComplexNetworkable"/>.</returns>
	/// <exception cref="InvalidOperationException">Thrown when reading the <see cref="ComplexNetworkable"/> has failed.</exception>
	public ComplexNetworkable ReadComplexNetworkable()
	{
		var networkId = ReadInt32();
		var type = ReadType( out var genericArguments );

		ComplexNetworkable? complexNetworkable;
		if ( genericArguments.Length > 0 )
			complexNetworkable = ITypeLibrary.Instance.Create<ComplexNetworkable?>( type, genericArguments );
		else
			complexNetworkable = ITypeLibrary.Instance.Create<ComplexNetworkable?>( type );

		if ( complexNetworkable is null )
		{
			ILogger.Instance.Error( "Failed to read {0} (instance creation failed).", nameof( ComplexNetworkable ) );
			return null!;
		}
		else
			complexNetworkable.NetworkId = networkId;

		complexNetworkable.Deserialize( this );
		return complexNetworkable;
	}

	/// <summary>
	/// Reads an instance of <see cref="ComplexNetworkable"/> and casts it to <see ref="T"/>.
	/// </summary>
	/// <typeparam name="T">The <see cref="ComplexNetworkable"/> type to cast into.</typeparam>
	/// <returns>The parsed <see cref="ComplexNetworkable"/>.</returns>
	/// <exception cref="InvalidOperationException">Thrown when reading the <see cref="ComplexNetworkable"/> has failed.</exception>
	public T ReadComplexNetworkable<T>()
	{
		var complexNetworkable = ReadComplexNetworkable();
		if ( complexNetworkable is not T expectedType )
		{
			ILogger.Instance.Error( "Failed to read {0} ({1} is not assignable to {2})", nameof( ComplexNetworkable ), complexNetworkable.GetType(), typeof( T ) );
			return default!;
		}

		return expectedType;
	}
}
