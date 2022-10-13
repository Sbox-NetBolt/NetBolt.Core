using System;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A <see cref="NetworkMessage"/> containing information to call a method from a different realm.
/// </summary>
public sealed class RpcCallMessage : NetworkMessage
{
	/// <summary>
	/// The unique identifier for the <see cref="RpcCallMessage"/>.
	/// </summary>
	public Guid CallGuid { get; private set; }
	/// <summary>
	/// The class name this <see cref="RpcCallMessage"/> came from.
	/// </summary>
	public string ClassName { get; private set; }
	/// <summary>
	/// The name of the method to call in <see cref="ClassName"/>.
	/// </summary>
	public string MethodName { get; private set; }
	/// <summary>
	/// The <see cref="BaseNetworkable"/>s network identifier to call the <see cref="MethodName"/> with.
	/// </summary>
	public int NetworkId { get; private set; }
	/// <summary>
	/// The parameters to send to the <see cref="MethodName"/>.
	/// </summary>
	public INetworkable[] Parameters { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="RpcCallMessage"/>.
	/// </summary>
	public RpcCallMessage()
	{
		CallGuid = Guid.Empty;
		ClassName = string.Empty;
		MethodName = string.Empty;
		NetworkId = -1;
		Parameters = Array.Empty<INetworkable>();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="RpcCallMessage"/> with all of the required information.
	/// </summary>
	/// <param name="respondable">Whether or not the creator of the <see cref="RpcCallMessage"/> is expecting a response.</param>
	/// <param name="entityType">The C# type that contains the method to call.</param>
	/// <param name="baseNetworkable">An instance of <see cref="BaseNetworkable"/> that the RPC is being called on.</param>
	/// <param name="methodName">The name of the method to call.</param>
	/// <param name="parameters">The parameters to pass to the method.</param>
	public RpcCallMessage( bool respondable, Type entityType, BaseNetworkable? baseNetworkable, string methodName,
		params INetworkable[] parameters )
	{
		CallGuid = respondable ? Guid.NewGuid() : Guid.Empty;
		ClassName = entityType.Name;
		if ( baseNetworkable is null )
			NetworkId = -1;
		else
			NetworkId = baseNetworkable.NetworkId;

		MethodName = methodName;
		Parameters = parameters;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="RpcCallMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		CallGuid = reader.ReadGuid();
		ClassName = reader.ReadString();
		MethodName = reader.ReadString();
		NetworkId = reader.ReadInt32();

		Parameters = new INetworkable[reader.ReadInt32()];
		for ( var i = 0; i < Parameters.Length; i++ )
			Parameters[i] = reader.ReadNetworkable();
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="RpcCallMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( CallGuid );
		writer.Write( ClassName );
		writer.Write( MethodName );
		writer.Write( NetworkId );

		writer.Write( Parameters.Length );
		foreach ( var argument in Parameters )
			writer.WriteNetworkable( argument );
	}
	
	/// <summary>
	/// Returns a string that represents the <see cref="RpcCallMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="RpcCallMessage"/>.</returns>
	public override string ToString()
	{
		return nameof(RpcCallMessage);
	}
}
