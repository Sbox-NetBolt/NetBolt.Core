using System;
using Serilog;
using Serilog.Core;

namespace NetBolt.Server;

/// <summary>
/// An abstraction layer for logging.
/// </summary>
public static class Log
{
	/// <summary>
	/// The Serilog logger instance.
	/// </summary>
	private static Logger _logger = null!;

	/// <summary>
	/// Initializes the server-side Serilog logger.
	/// </summary>
	internal static void Initialize()
	{
		_logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.Console()
			.WriteTo.File( "logs/log.txt", rollingInterval: RollingInterval.Day )
			.CreateLogger();
	}

	/// <summary>
	/// Disposes the server-side Serilog logger.
	/// </summary>
	internal static void Dispose()
	{
		_logger.Dispose();
	}

	/// <summary>
	/// Logs information.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void Info( string message )
	{
		_logger.Information( "{A}", message );
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
		_logger.Information( messageTemplate, obj );
	}

	/// <summary>
	/// Logs information.
	/// </summary>
	/// <param name="obj">The first object to log.</param>
	/// <typeparam name="T">The type of <see ref="obj"/>.</typeparam>
	public static void Info<T>( T obj )
	{
		_logger.Information( "{A}", obj );
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
		_logger.Information( messageTemplate, obj0, obj1 );
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
		_logger.Information( "{A}\t{B}", obj0, obj1 );
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
		_logger.Information( messageTemplate, obj0, obj1, obj2 );
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
		_logger.Information( "{A}\t{B}\t{C}", obj0, obj1, obj2 );
	}

	/// <summary>
	/// Logs a warning.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="exception">The <see cref="Exception"/> attached to this warning.</param>
	public static void Warning( string message, Exception? exception = null )
	{
		_logger.Warning( exception, "{A}", message );
	}

	/// <summary>
	/// Logs a warning.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	public static void Warning( Exception exception )
	{
		_logger.Warning( exception, "An exception occurred during runtime" );
	}

	/// <summary>
	/// Logs an error.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="exception">The <see cref="Exception"/> attached to this error.</param>
	public static void Error( string message, Exception? exception = null )
	{
		_logger.Information( exception, "{A}", message );
	}

	/// <summary>
	/// Logs an error.
	/// </summary>
	/// <param name="exception">The exception to log.</param>
	public static void Error( Exception exception )
	{
		_logger.Information( exception, "An exception occurred during runtime" );
	}

	/// <summary>
	/// Logs an <see cref="Exception"/> then throws it.
	/// </summary>
	/// <param name="exception">The <see cref="Exception"/> to log then throw.</param>
	/// <param name="throwException">Whether or not to throw the exception given.</param>
	/// <exception cref="Exception">The <see cref="Exception"/> passed.</exception>
	public static void Fatal( Exception exception, bool throwException = true )
	{
		_logger.Fatal( exception, "A fatal exception occurred during runtime" );
		if ( throwException )
			throw exception;
	}
}
