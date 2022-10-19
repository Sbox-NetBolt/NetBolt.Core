using System;

namespace NetBolt.Shared.Exceptions;

/// <summary>
/// Represents an exception specific to NetBolt.
/// </summary>
public class NetBoltException : Exception
{
	/// <summary>
	/// Initializes a new instance of <see cref="NetBoltException"/> with a specified error message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public NetBoltException( string message ) : base( message )
	{
	}
}
