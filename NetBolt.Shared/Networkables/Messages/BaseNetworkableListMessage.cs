using System.Collections.Generic;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a list of <see cref="ComplexNetworkable"/>s to notify the client about.
/// </summary>
public sealed class BaseNetworkableListMessage : NetworkMessage
{
	/// <summary>
	/// A list of all <see cref="ComplexNetworkable"/>s to let the client know about.
	/// </summary>
	public IReadOnlyList<ComplexNetworkable> BaseNetworkables { get; private set; } = null!;

	/// <summary>
	/// Initializes a default instance of <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	[ClientOnly]
	public BaseNetworkableListMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="BaseNetworkableListMessage"/> with the list of <see cref="ComplexNetworkable"/> to notify a client about.
	/// </summary>
	/// <param name="baseNetworkableList">The list of <see cref="ComplexNetworkable"/> to notify a client about.</param>
	[ServerOnly]
	public BaseNetworkableListMessage( IReadOnlyList<ComplexNetworkable> baseNetworkableList )
	{
		BaseNetworkables = baseNetworkableList;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		var baseNetworkables = new List<ComplexNetworkable> { Capacity = reader.ReadInt32() };
		for ( var i = 0; i < baseNetworkables.Capacity; i++ )
			baseNetworkables.Add( reader.ReadBaseNetworkable() );

		BaseNetworkables = baseNetworkables;
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="BaseNetworkableListMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( BaseNetworkables.Count );
		foreach ( var complexNetworkable in BaseNetworkables )
			writer.Write( complexNetworkable );
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
