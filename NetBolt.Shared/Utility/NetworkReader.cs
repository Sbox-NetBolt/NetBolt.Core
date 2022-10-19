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
	/// Reads an instance of <see cref="INetworkable"/>.
	/// </summary>
	/// <returns>The parsed <see cref="INetworkable"/>.</returns>
	/// <exception cref="InvalidOperationException">Thrown when reading the <see cref="INetworkable"/> has failed.</exception>
	public INetworkable ReadNetworkable()
	{
		var typeName = ReadString();
		var type = IGlue.Instance.TypeLibrary.GetTypeByName( typeName );
		if ( type is null )
		{
			IGlue.Instance.Logger.Error( "Failed to read networkable (\"{0}\" does not exist)", typeName );
			return null!;
		}

		INetworkable? networkable;
		if ( type.IsGenericType )
		{
			var genericCount = ReadInt32();
			var genericTypes = new Type[genericCount];
			for ( var i = 0; i < genericCount; i++ )
			{
				var genericTypeName = ReadString();
				var genericType = IGlue.Instance.TypeLibrary.GetTypeByName( genericTypeName );
				if ( genericType is null )
				{
					IGlue.Instance.Logger.Error( "Failed to read networkable (Generic argument \"{0}\" does not exist).", genericTypeName );
					return null!;
				}

				genericTypes[i] = genericType;
			}

			networkable = IGlue.Instance.TypeLibrary.Create<INetworkable>( type, genericTypes );
		}
		else
			networkable = IGlue.Instance.TypeLibrary.Create<INetworkable>( type );

		if ( networkable is null )
		{
			IGlue.Instance.Logger.Error( "Failed to read networkable (instance creation failed)." );
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
		if ( typeof( T ).IsAssignableTo( typeof( BaseNetworkable ) ) )
			return ReadBaseNetworkable<T>();

		var networkable = ReadNetworkable();
		if ( networkable is not T outputNetworkable )
		{
			IGlue.Instance.Logger.Error( "Failed to read networkable ({0} is not assignable to {1}).", networkable.GetType(), typeof( T ) );
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
	/// Reads an instance of <see cref="BaseNetworkable"/>.
	/// </summary>
	/// <returns>The parsed <see cref="BaseNetworkable"/>.</returns>
	/// <exception cref="InvalidOperationException">Thrown when reading the <see cref="BaseNetworkable"/> has failed.</exception>
	public BaseNetworkable ReadBaseNetworkable()
	{
		var networkId = ReadInt32();
		var typeName = ReadString();

		var type = IGlue.Instance.TypeLibrary.GetTypeByName( typeName );
		if ( type is null )
		{
			IGlue.Instance.Logger.Error( "Failed to read {0} (\"{1}\" does not exist)", nameof( BaseNetworkable ), typeName );
			return null!;
		}

		var baseNetworkable = IGlue.Instance.TypeLibrary.Create<BaseNetworkable?>( type );
		if ( baseNetworkable is null )
		{
			IGlue.Instance.Logger.Error( "Failed to read {0} (instance creation failed).", nameof( BaseNetworkable ) );
			return null!;
		}
		else
			baseNetworkable.NetworkId = networkId;

		baseNetworkable.Deserialize( this );
		return baseNetworkable;
	}

	/// <summary>
	/// Reads an instance of <see cref="BaseNetworkable"/> and casts it to <see ref="T"/>.
	/// </summary>
	/// <typeparam name="T">The <see cref="BaseNetworkable"/> type to cast into.</typeparam>
	/// <returns>The parsed <see cref="BaseNetworkable"/>.</returns>
	/// <exception cref="InvalidOperationException">Thrown when reading the <see cref="BaseNetworkable"/> has failed.</exception>
	public T ReadBaseNetworkable<T>()
	{
		var baseNetworkable = ReadBaseNetworkable();
		if ( baseNetworkable is not T expectedType )
		{
			IGlue.Instance.Logger.Error( "Failed to read {0} ({1} is not assignable to {2})", nameof( BaseNetworkable ), baseNetworkable.GetType(), typeof( T ) );
			return default!;
		}

		return expectedType;
	}
}
