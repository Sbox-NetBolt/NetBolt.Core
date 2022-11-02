namespace NetBolt.Shared;

/// <summary>
/// The glue required to have full logging functionality in NetBolt.
/// <remarks>Message templates implement parameters as curly braces ({}) with a zero-indexed number inside it.</remarks>
/// </summary>
public interface ILogger
{
	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <typeparam name="T">The type of <see ref="obj0"/>.</typeparam>
	void Warning<T>( string messageTemplate, T obj0 );

	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="message">The message to log.</param>
	void Error( string message );
	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <typeparam name="T">The type of <see ref="obj0"/>.</typeparam>
	void Error<T>( string messageTemplate, T obj0 );
	/// <summary>
	/// Logs a error message.
	/// </summary>
	/// <param name="messageTemplate">The message template.</param>
	/// <param name="obj0">The first object to input to the template.</param>
	/// <param name="obj1">The second object to input to the template.</param>
	/// <typeparam name="T1">The type of <see ref="obj0"/>.</typeparam>
	/// <typeparam name="T2">The type of <see ref="obj1"/>.</typeparam>
	void Error<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 );
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
	void Error<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 );

	/// <summary>
	/// The instance to access for logger glue functionality.
	/// </summary>
	public static ILogger Instance { get; internal set; } = null!;
}
