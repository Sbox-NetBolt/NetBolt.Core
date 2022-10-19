namespace NetBolt.Shared.RemoteProcedureCalls;

/// <summary>
/// Represents a state the Rpc resulted in.
/// </summary>
public enum RpcCallState : byte
{
	/// <summary>
	/// The RPC completed.
	/// </summary>
	Completed,
	/// <summary>
	/// The RPC failed.
	/// </summary>
	Failed
}
