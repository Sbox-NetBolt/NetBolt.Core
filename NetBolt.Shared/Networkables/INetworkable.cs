using NetBolt.Shared.Utility;
using System;

namespace NetBolt.Shared.Networkables;

/// <summary>
/// Contract to define something that can be networked.
/// </summary>
public interface INetworkable
{
	/// <summary>
	/// The unique identifier of the networkable.
	/// <remarks>A value of 0 means it does not support this.</remarks>
	/// </summary>
	int NetworkId { get; }
	/// <summary>
	/// Whether or not the <see cref="Equals"/> method is supported.
	/// </summary>
	bool SupportEquals { get; }
	/// <summary>
	/// Whether or not the <see cref="Lerp"/> method is supported.
	/// </summary>
	bool SupportLerp { get; }

	/// <summary>
	/// Returns whether or not the <see cref="INetworkable"/> has changed.
	/// </summary>
	/// <returns>Whether or not the <see cref="INetworkable"/> has changed.</returns>
	bool Changed();
	/// <summary>
	/// Returns whether or not the <see cref="INetworkable"/> instance is the same as another.
	/// </summary>
	/// <param name="oldValue">The old value.</param>
	/// <returns>Whether or not the <see cref="INetworkable"/> instance is the same as another.</returns>
	bool Equals( INetworkable? oldValue );
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

	/// <summary>
	/// Checks whether or not a change has occurred in a networkable.
	/// </summary>
	/// <param name="type">The base type of the networkable.</param>
	/// <param name="oldValue">The old networkable value.</param>
	/// <param name="newValue">The new networkable value.</param>
	/// <param name="checkInstanceChanged">Whether or not to check if the instance has changed.</param>
	/// <typeparam name="T">The type of the networkable values passed.</typeparam>
	/// <returns>Whether or not there is a change.</returns>
	public static bool HasChanged<T>( Type type, T? oldValue, T? newValue, bool checkInstanceChanged ) where T : INetworkable
	{
		if ( oldValue is null && newValue is null )
			return false;

		if ( (oldValue is null && newValue is not null) || (oldValue is not null && newValue is null) )
			return true;

		if ( oldValue!.NetworkId != newValue!.NetworkId )
			return true;

		var oldType = oldValue?.GetType() ?? typeof( object );
		var newType = newValue?.GetType() ?? typeof( object );

		if ( oldType != newType )
			return true;

		if ( newValue is not null && newValue.SupportEquals )
			return !newValue.Equals( oldValue );

		return (ITypeLibrary.Instance.IsClass( type ) && !ReferenceEquals( oldValue, newValue )) ||
			(checkInstanceChanged && newValue is not null && newValue.Changed());
	}
}
