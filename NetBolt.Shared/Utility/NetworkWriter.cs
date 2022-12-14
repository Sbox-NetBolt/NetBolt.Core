using System;
using System.IO;
using System.Numerics;
using NetBolt.Shared.Networkables;

namespace NetBolt.Shared.Utility;

/// <summary>
/// Writes any data relating to networking and primitive types.
/// </summary>
public sealed class NetworkWriter : BinaryWriter
{
	private readonly bool UsingCachedTypes;

	/// <summary>
	/// Initializes a new instance of <see cref="NetworkWriter"/> with the <see cref="Stream"/> to write to.
	/// </summary>
	/// <param name="output">The underlying <see cref="Stream"/> to write to.</param>
	/// <param name="useCachedTypes">Whether or not to use cached <see cref="INetworkable"/> types.</param>
	public NetworkWriter( Stream output, bool useCachedTypes = true ) : base( output )
	{
		UsingCachedTypes = useCachedTypes;
	}

	/// <summary>
	/// Writes a 16 byte Globally Unique Identifier (<see cref="Guid"/>).
	/// </summary>
	/// <param name="guid">The instance of <see cref="Guid"/> to write.</param>
	public void Write( Guid guid )
	{
		Write( guid.ToByteArray() );
	}

	/// <summary>
	/// Writes a 4 float <see cref="Quaternion"/>.
	/// </summary>
	/// <param name="quaternion">The instance of <see cref="Quaternion"/> to write.</param>
	public void Write( Quaternion quaternion )
	{
		Write( quaternion.X );
		Write( quaternion.Y );
		Write( quaternion.Z );
		Write( quaternion.W );
	}

	/// <summary>
	/// Writes a 3 float <see cref="System.Numerics.Vector3"/>.
	/// </summary>
	/// <param name="vector3">The instance of <see cref="System.Numerics.Vector3"/> to write.</param>
	public void Write( System.Numerics.Vector3 vector3 )
	{
		Write( vector3.X );
		Write( vector3.Y );
		Write( vector3.Z );
	}

	/// <summary>
	/// Writes an instance of <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type to write.</param>
	public void Write( Type type )
	{
		var isNetworkableType = type.IsAssignableTo( typeof( INetworkable ) ) && UsingCachedTypes;
		Write( isNetworkableType );

		if ( isNetworkableType )
			Write( ITypeLibrary.Instance.GetIdentifierFromNetworkableType( type ) );
		else
			Write( type.FullName ?? type.Name );

		if ( type.IsGenericType )
		{
			var genericArguments = ITypeLibrary.Instance.GetGenericArguments( type );
			Write( genericArguments.Length );
			foreach ( var genericType in genericArguments )
			{
				isNetworkableType = type.IsAssignableTo( typeof( INetworkable ) );
				Write( isNetworkableType );

				if ( isNetworkableType )
					Write( ITypeLibrary.Instance.GetIdentifierFromNetworkableType( genericType ) );
				else
					Write( genericType.FullName ?? genericType.Name );
			}
		}
	}

	/// <summary>
	/// Writes an instance of <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="networkable">The instance of <see cref="INetworkable"/> to write.</param>
	public void Write( INetworkable networkable )
	{
		Write( networkable.GetType() );
		networkable.Serialize( this );
	}

	/// <summary>
	/// Writes an instance of <see cref="ComplexNetworkable"/>.
	/// </summary>
	/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> to write.</param>
	public void Write( ComplexNetworkable complexNetworkable )
	{
		Write( complexNetworkable.NetworkId );
		Write( (INetworkable)complexNetworkable );
	}

	/// <summary>
	/// Writes the changes of an <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="networkable">The instance of <see cref="INetworkable"/> to write changes.</param>
	public void WriteChanges( ref INetworkable networkable )
	{
		networkable.SerializeChanges( this );
	}
}
