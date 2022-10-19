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
	/// <summary>
	/// Initializes a new instance of <see cref="NetworkWriter"/> with the <see cref="Stream"/> to write to.
	/// </summary>
	/// <param name="output">The underlying <see cref="Stream"/> to write to.</param>
	public NetworkWriter( Stream output ) : base( output )
	{
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
	/// Writes an instance of <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="networkable">The instance of <see cref="INetworkable"/> to write.</param>
	public void WriteNetworkable( INetworkable networkable )
	{
		var networkableType = networkable.GetType();
		Write( networkableType.Name );
		if ( networkableType.IsGenericType )
		{
			var genericArguments = IGlue.Instance.TypeLibrary.GetGenericArguments( networkableType );
			Write( genericArguments.Length );
			foreach ( var type in genericArguments )
				Write( type.Name );
		}

		networkable.Serialize( this );
	}

	/// <summary>
	/// Writes an instance of <see cref="BaseNetworkable"/>.
	/// </summary>
	/// <param name="baseNetworkable">The <see cref="BaseNetworkable"/> to write.</param>
	public void WriteBaseNetworkable( BaseNetworkable baseNetworkable )
	{
		Write( baseNetworkable.NetworkId );
		WriteNetworkable( baseNetworkable );
	}

	/// <summary>
	/// Writes the changes of an <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="networkable">The instance of <see cref="INetworkable"/> to write changes.</param>
	public void WriteNetworkableChanges( ref INetworkable networkable )
	{
		networkable.SerializeChanges( this );
	}
}