using NetBolt.Shared.Utility;

namespace NetBolt.Shared.Networkables;

/// <summary>
/// Contract to define something that can be networked.
/// </summary>
public interface INetworkable
{
	/// <summary>
	/// Returns whether or not the <see cref="INetworkable"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="INetworkable"/> has changed.</returns>
	bool Changed();

	/// <summary>
	/// Lerps a <see cref="INetworkable"/> between two values.
	/// </summary>
	/// <param name="fraction">The fraction to lerp at.</param>
	/// <param name="oldValue">The old value.</param>
	/// <param name="newValue">The new value.</param>
	void Lerp( float fraction, INetworkable oldValue, INetworkable newValue );
	/// <summary>
	/// Deserializes all information relating to the <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	void Deserialize( NetworkReader reader );
	/// <summary>
	/// Deserializes all changes relating to the <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="reader">The reader to read from.</param>
	void DeserializeChanges( NetworkReader reader );
	/// <summary>
	/// Serializes all information relating to the <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	void Serialize( NetworkWriter writer );
	/// <summary>
	/// Serializes all changes relating to the <see cref="INetworkable"/>.
	/// </summary>
	/// <param name="writer">The writer to write to.</param>
	void SerializeChanges( NetworkWriter writer );
}
