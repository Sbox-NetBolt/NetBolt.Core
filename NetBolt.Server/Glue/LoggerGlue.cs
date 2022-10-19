using NetBolt.Shared;

namespace NetBolt.Server.Glue;

/// <summary>
/// The glue required for logging in NetBolt.
/// </summary>
internal class LoggerGlue : ILogger
{
	/// <inheritdoc/>
	public void Error( string message )
	{
		Log.Error( message );
	}

	/// <inheritdoc/>
	public void Error<T>( string messageTemplate, T obj0 )
	{
		Log.Error( messageTemplate, obj0 );
	}

	/// <inheritdoc/>
	public void Error<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 )
	{
		Log.Error( messageTemplate, obj0, obj1 );
	}

	/// <inheritdoc/>
	public void Error<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 )
	{
		Log.Error( messageTemplate, obj0, obj1, obj2 );
	}

	/// <inheritdoc/>
	public void Warning<T>( string messageTemplate, T obj0 )
	{
		Log.Warning( messageTemplate, obj0 );
	}
}
