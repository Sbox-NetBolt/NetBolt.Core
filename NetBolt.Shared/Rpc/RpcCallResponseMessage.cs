using System;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.RemoteProcedureCalls;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A <see cref="NetworkMessage"/> containing a response to a <see cref="RpcCallMessage"/>.
/// </summary>
public sealed class RpcCallResponseMessage : NetworkMessage
{
	/// <summary>
	/// The unique identifier of the <see cref="RpcCallMessage"/> this message is responding to.
	/// </summary>
	public Guid CallGuid { get; private set; }
	/// <summary>
	/// The state of the executed <see cref="RpcCallMessage"/>.
	/// </summary>
	public RpcCallState State { get; private set; }
	/// <summary>
	/// The return value from the <see cref="RpcCallMessage"/>.
	/// </summary>
	public INetworkable? ReturnValue { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="RpcCallResponseMessage"/>.
	/// </summary>
	public RpcCallResponseMessage()
	{
		CallGuid = Guid.Empty;
		State = RpcCallState.Failed;
		ReturnValue = null;
	}

	/// <summary>
	/// Initializes a new instance of <see cref="RpcCallResponseMessage"/> with all of the required information.
	/// </summary>
	/// <param name="callGuid">The <see cref="Guid"/> this message is responding to.</param>
	/// <param name="state">The resulting state of the RPC call.</param>
	/// <param name="returnValue">The return value of the RPC if applicable.</param>
	public RpcCallResponseMessage( Guid callGuid, RpcCallState state, INetworkable? returnValue = null )
	{
		CallGuid = callGuid;
		State = state;
		ReturnValue = returnValue;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="RpcCallResponseMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
		CallGuid = reader.ReadGuid();
		State = (RpcCallState)reader.ReadByte();
		ReturnValue = reader.ReadBoolean() ? reader.ReadNetworkable() : null;
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="RpcCallResponseMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( CallGuid );
		writer.Write( (byte)State );

		var hasReturnValue = ReturnValue is not null;
		writer.Write( hasReturnValue );
		if ( hasReturnValue )
			writer.Write( ReturnValue! );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="RpcCallResponseMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="RpcCallResponseMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( RpcCallResponseMessage );
	}
}
