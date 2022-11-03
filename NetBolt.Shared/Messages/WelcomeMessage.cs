using NetBolt.Shared.Utility;
using System;
using System.Collections.Generic;

namespace NetBolt.Shared.Messages;

/// <summary>
/// A server to client <see cref="NetworkMessage"/> that signifies to the client that they have joined the server.
/// </summary>
public sealed class WelcomeMessage : NetworkMessage
{
	/// <summary>
	/// The tick rate that the server is running at.
	/// </summary>
	public int TickRate { get; private set; }
	/// <summary>
	/// A welcome message for the client.
	/// </summary>
	public string Message { get; private set; } = "";
	/// <summary>
	/// A cache containing all potentially networkable types with an integer to represent them in data.
	/// </summary>
	public IReadOnlyDictionary<Type, ushort> TypeCacheMap = null!;

	/// <summary>
	/// Initializes a default instance of <see cref="WelcomeMessage"/>.
	/// </summary>
	[ClientOnly]
	public WelcomeMessage()
	{
	}

	/// <summary>
	/// Initializes a new instance of <see cref="WelcomeMessage"/> with the servers tick rate and a welcome message to the client.
	/// </summary>
	/// <param name="tickRate">The target tick rate of the server.</param>
	/// <param name="message">A welcome message for the client.</param>
	/// <param name="typeCacheMap">A cache containing all potentially networkable types.</param>
	[ServerOnly]
	public WelcomeMessage( int tickRate, string message, IReadOnlyDictionary<Type, ushort> typeCacheMap )
	{
		TickRate = tickRate;
		Message = message;
		TypeCacheMap = typeCacheMap;
	}

	/// <summary>
	/// Deserializes all information relating to the <see cref="WelcomeMessage"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	[ClientOnly]
	public override void Deserialize( NetworkReader reader )
	{
		TickRate = reader.ReadInt32();
		Message = reader.ReadString();

		var typeCacheMap = new Dictionary<Type, int>();
		var cacheCount = reader.ReadInt32();
		for ( var i = 0; i < cacheCount; i++ )
		{
			var typeName = reader.ReadString();
			var type = ITypeLibrary.Instance.GetTypeByName( typeName );
			if ( type is null )
			{
				ILogger.Instance.Error( "Failed to find type with name \"{0}\"", typeName );
				reader.ReadInt32();
				continue;
			}

			typeCacheMap.Add( type, reader.ReadUInt16() );
		}
	}

	/// <summary>
	/// Serializes all information relating to the <see cref="WelcomeMessage"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	[ServerOnly]
	public override void Serialize( NetworkWriter writer )
	{
		writer.Write( TickRate );
		writer.Write( Message );

		writer.Write( TypeCacheMap.Count );
		foreach ( var (type, id) in TypeCacheMap )
		{
			writer.Write( type.FullName ?? type.Name );
			writer.Write( id );
		}
	}

	/// <summary>
	/// Returns a string that represents the <see cref="WelcomeMessage"/>.
	/// </summary>
	/// <returns> string that represents the <see cref="WelcomeMessage"/>.</returns>
	public override string ToString()
	{
		return nameof( WelcomeMessage );
	}
}
