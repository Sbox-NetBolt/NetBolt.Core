using System.Collections.Generic;
using NetBolt.Shared.Networkables;
using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> containing a list of <see cref="ComplexNetworkable"/>s to notify the client about.
/// </summary>
public sealed class ComplexNetworkableListMessage : NetworkMessage
{
	/// <summary>
	/// A list of all <see cref="ComplexNetworkable"/>s to let the client know about.
	/// </summary>
	public IReadOnlyList<ComplexNetworkable> ComplexNetworkables { get; private set; } = null!;

	/// <summary>
	/// Initializes a default instance of <see cref="ComplexNetworkableListMessage"/>.
	/// </summary>
	[ClientOnly]
	public ComplexNetworkableListMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ComplexNetworkableListMessage"/> with the list of <see cref="ComplexNetworkable"/> to notify a client about.
	/// </summary>
	/// <param name="complexNetworkableList">The list of <see cref="ComplexNetworkable"/> to notify a client about.</param>
	[ServerOnly]
	public ComplexNetworkableListMessage( IReadOnlyList<ComplexNetworkable> complexNetworkableList )
	{
		ComplexNetworkables = complexNetworkableList;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="ComplexNetworkableListMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		var complexNetworkables = new List<ComplexNetworkable> { Capacity = reader.ReadInt32() };
		for ( var i = 0; i < complexNetworkables.Capacity; i++ )
			complexNetworkables.Add( reader.ReadComplexNetworkable() );

		ComplexNetworkables = complexNetworkables;
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="ComplexNetworkableListMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( ComplexNetworkables.Count );
		foreach ( var complexNetworkable in ComplexNetworkables )
			writer.Write( complexNetworkable );
	}

	/// <summary>
	/// Returns a string that represents the <see cref="ComplexNetworkableListMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="ComplexNetworkableListMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( ComplexNetworkableListMessage );
	}
}
