using System.Collections.Generic;
using NetBolt.Shared.Networkables;
#if SERVER
#endif
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a list of <see cref="BaseNetworkable"/>s to notify the client about.
/// </summary>
public sealed class BaseNetworkableListMessage : NetworkMessage
{
	/// <summary>
	/// A list of all <see cref="BaseNetworkable"/>s to let the client know about.
	/// </summary>
	public IReadOnlyList<BaseNetworkable> BaseNetworkables { get; private set; } = null!;

#if SERVER
	/// <summary>
	/// Initializes a new instance of <see cref="BaseNetworkableListMessage"/> with the list of <see cref="BaseNetworkable"/> to notify a client about.
	/// </summary>
	/// <param name="baseNetworkableList">The list of <see cref="BaseNetworkable"/> to notify a client about.</param>
	public BaseNetworkableListMessage( IReadOnlyList<BaseNetworkable> baseNetworkableList )
	{
		BaseNetworkables = baseNetworkableList;
	}
#endif

	/// <summary>
	/// Deserializes all information relating to the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	public override void Deserialize( NetworkReader reader )
	{
#if CLIENT
		var baseNetworkables = new List<BaseNetworkable> {Capacity = reader.ReadInt32()};
		for ( var i = 0; i < baseNetworkables.Capacity; i++ )
			baseNetworkables.Add( reader.ReadBaseNetworkable() );

		BaseNetworkables = baseNetworkables;
#endif
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	public override void Serialize( NetworkWriter writer )
	{
#if SERVER
		writer.Write( BaseNetworkables.Count );
		foreach ( var baseNetworkable in BaseNetworkables )
			writer.WriteBaseNetworkable( baseNetworkable );
#endif
	}

	/// <summary>
	/// Returns a string that represents the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="BaseNetworkableListMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( BaseNetworkableListMessage );
	}
}
