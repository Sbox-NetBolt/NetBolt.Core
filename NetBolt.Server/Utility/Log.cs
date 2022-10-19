using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace NetBolt.Server;

/// <summary>
/// An abstraction layer for logging.
/// </summary>
public static class Log
{
#if !NOLOGS
	/// <summary>
	/// The Serilog logger instance.
	/// </summary>
	private static Logger _logger = null!;
#endif

	/// <summary>
	/// Initializes the server-side Serilog logger.
	/// </summary>
	internal static void Initialize()
	{
#if !NOLOGS
		var config = new LoggerConfiguration()
			.WriteTo.Console()
			.WriteTo.File( "logs/log.txt", rollingInterval: RollingInterval.Day );

		switch ( NetBoltGame.Current.Options.LogLevel )
		{
			case LogEventLevel.Verbose:
				config.MinimumLevel.Verbose();
				break;
			case LogEventLevel.Debug:
				config.MinimumLevel.Debug();
				break;
			case LogEventLevel.Information:
				config.MinimumLevel.Information();
				break;
			case LogEventLevel.Warning:
				config.MinimumLevel.Warning();
				break;
			case LogEventLevel.Error:
				config.MinimumLevel.Error();
				break;
			case LogEventLevel.Fatal:
				config.MinimumLevel.Fatal();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		_logger = config.CreateLogger();
#endif
	}

	/// <summary>
	/// Disposes the server-side Serilog logger.
	/// </summary>
	internal static void Dispose()
	{
#if !NOLOGS
		_logger.Dispose();
#endif
	}

	/// <summary>
	/// Logs information.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Info( string message )
	{
#if !NOLOGS
		_logger.Information( "{A}", message );
#endif
	}

	/// <summary>
	/// Logs information
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj">The first object to input to the template.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Info<T>( string messageTemplate, T obj )
	{
#if !NOLOGS
		_logger.Information( messageTemplate, obj );
#endif
	}

	/// <summary>
	/// Logs information.
	/// </summary>
	/// <param name="obj">The first object to log.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	public static void Info<T>( T obj )
	{
#if !NOLOGS
		_logger.Information( "{A}", obj );
#endif
	}

	/// <summary>
	/// Logs information
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Info<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Information( messageTemplate, obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs information.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	public static void Info<T1, T2>( T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Information( "{A}\t{B}", obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs information
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <param name="obj2">The third object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Info<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Information( messageTemplate, obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs information.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <param name="obj2">The third object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	public static void Info<T1, T2, T3>( T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Information( "{A}\t{B}\t{C}", obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Warning( string message )
	{
#if !NOLOGS
		_logger.Warning( "{A}", message );
#endif
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj">The first object to input to the template.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Warning<T>( string messageTemplate, T obj )
	{
#if !NOLOGS
		_logger.Warning( messageTemplate, obj );
#endif
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="obj">The first object to log.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	public static void Warning<T>( T obj )
	{
#if !NOLOGS
		_logger.Warning( "{A}", obj );
#endif
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Warning<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Warning( messageTemplate, obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	public static void Warning<T1, T2>( T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Warning( "{A}\t{B}", obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <param name="obj2">The third object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Warning<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Warning( messageTemplate, obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <param name="obj2">The third object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	public static void Warning<T1, T2, T3>( T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Warning( "{A}\t{B}\t{C}", obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Error( string message )
	{
#if !NOLOGS
		_logger.Error( "{A}", message );
#endif
	}

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj">The first object to input to the template.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Error<T>( string messageTemplate, T obj )
	{
#if !NOLOGS
		_logger.Error( messageTemplate, obj );
#endif
	}

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="obj">The first object to log.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	public static void Error<T>( T obj )
	{
#if !NOLOGS
		_logger.Error( "{A}", obj );
#endif
	}

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Error<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Error( messageTemplate, obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	public static void Error<T1, T2>( T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Error( "{A}\t{B}", obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <param name="obj2">The third object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Error<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Error( messageTemplate, obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <param name="obj2">The third object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	public static void Error<T1, T2, T3>( T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Error( "{A}\t{B}\t{C}", obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs an <see cref="Exception"/> then throws it.
	/// </summary>
	/// <param name="exception">The <see cref="Exception"/> to log then throw.</param>
	/// <param name="throwException">Whether or not to throw the exception given.</param>
	/// <exception cref="Exception">The <see cref="Exception"/> passed.</exception>
	public static void Fatal( Exception exception, bool throwException = true )
	{
#if !NOLOGS
		_logger.Fatal( exception, "A fatal exception occurred during runtime" );
		if ( throwException )
			throw exception;
#endif
	}

	/// <summary>
	/// Logs a debug message.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Debug( string message )
	{
#if !NOLOGS
		_logger.Debug( "{A}", message );
#endif
	}

	/// <summary>
	/// Logs a debug message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj">The first object to input to the template.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Debug<T>( string messageTemplate, T obj )
	{
#if !NOLOGS
		_logger.Debug( messageTemplate, obj );
#endif
	}

	/// <summary>
	/// Logs a debug message.
	/// </summary>
	/// <param name="obj">The first object to log.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	public static void Debug<T>( T obj )
	{
#if !NOLOGS
		_logger.Debug( "{A}", obj );
#endif
	}

	/// <summary>
	/// Logs a debug message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Debug<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Debug( messageTemplate, obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a debug message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	public static void Debug<T1, T2>( T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Debug( "{A}\t{B}", obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a debug message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <param name="obj2">The third object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Debug<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Debug( messageTemplate, obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs a debug message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <param name="obj2">The third object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	public static void Debug<T1, T2, T3>( T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Debug( "{A}\t{B}\t{C}", obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs a verbose message.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Verbose( string message )
	{
#if !NOLOGS
		_logger.Verbose( "{A}", message );
#endif
	}

	/// <summary>
	/// Logs a verbose message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj">The first object to input to the template.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Verbose<T>( string messageTemplate, T obj )
	{
#if !NOLOGS
		_logger.Verbose( messageTemplate, obj );
#endif
	}

	/// <summary>
	/// Logs a verbose message.
	/// </summary>
	/// <param name="obj">The first object to log.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	public static void Verbose<T>( T obj )
	{
#if !NOLOGS
		_logger.Verbose( "{A}", obj );
#endif
	}

	/// <summary>
	/// Logs a verbose message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Verbose<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Verbose( messageTemplate, obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a verbose message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	public static void Verbose<T1, T2>( T1 obj0, T2 obj1 )
	{
#if !NOLOGS
		_logger.Verbose( "{A}\t{B}", obj0, obj1 );
#endif
	}

	/// <summary>
	/// Logs a verbose message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <param name="obj2">The third object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	[MessageTemplateFormatMethod( "messageTemplate" )]
	public static void Verbose<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Verbose( messageTemplate, obj0, obj1, obj2 );
#endif
	}

	/// <summary>
	/// Logs a verbose message.
	/// </summary>
	/// <param name="obj0">The first object to log.</param>
	/// <param name="obj1">The second object to log.</param>
	/// <param name="obj2">The third object to log.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	/// <typeparam name="T3">The type of <see ref="obj2"/>.</typeparam>
	public static void Verbose<T1, T2, T3>( T1 obj0, T2 obj1, T3 obj2 )
	{
#if !NOLOGS
		_logger.Verbose( "{A}\t{B}\t{C}", obj0, obj1, obj2 );
#endif
	}
}
