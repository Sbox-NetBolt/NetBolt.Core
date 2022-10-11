using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Utility;
using NetBolt.WebSocket.Enums;

namespace NetBolt.Server;

public partial class BotClient
{
	/// <summary>
	/// A bots own client-side message handlers.
	/// </summary>
	private static readonly Dictionary<Type, Action<BotClient, NetworkMessage>> MessageHandlers = new();
	
	/// <summary>
	/// Serializes a message and sends the data to the client.
	/// </summary>
	/// <param name="message">The message to send to the client.</param>
	public void QueueSend( NetworkMessage message )
	{
		if ( !MessageHandlers.TryGetValue( message.GetType(), out var cb ) )
		{
			Log.Error( $"Unhandled message type {message.GetType()} for bot." );
			return;
		}

		cb.Invoke( this, message );
	}

	/// <summary>
	/// Disconnects the client from the server.
	/// </summary>
	/// <param name="reason">The reason for the disconnect.</param>
	/// <param name="strReason">The string representation of the reason for the disconnect.</param>
	/// <param name="error">The error associated with the disconnect if applicable.</param>
	/// <returns>The async task that spawns from the invoke.</returns>
	/// <exception cref="NotImplementedException">Always thrown due to not being able to disconnect a bot.</exception>
	public Task DisconnectAsync( WebSocketDisconnectReason reason = WebSocketDisconnectReason.Requested, string strReason = "",
		WebSocketError? error = null )
	{
		throw new NotImplementedException();
	}
	
	/// <summary>
	/// Acts as the main loop for the client to handle its read/write logic.
	/// </summary>
	/// <returns>The async task that spawns from the invoke.</returns>
	public Task HandleAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Pings the client.
	/// </summary>
	/// <param name="timeout">The time in seconds before the ping is timed out.</param>
	/// <returns>The async task that spawns from the invoke. The return value of the task is the amount of time taken in milliseconds for the round trip. -1 will be returned if it timed out.</returns>
	public ValueTask<int> PingAsync( int timeout = int.MaxValue )
	{
		return new ValueTask<int>( 0 );
	}

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Binary"/> message to the client.
	/// </summary>
	/// <param name="bytes">The binary data to send.</param>
	/// <exception cref="NotImplementedException">Always thrown due to not being able to send anything other than <see cref="NetworkMessage"/>s to bots.</exception>
	public void QueueSend( byte[] bytes )
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Sends a <see cref="WebSocketOpCode.Text"/> message to the client.
	/// </summary>
	/// <param name="message">The message to send.</param>
	/// <exception cref="NotImplementedException">Always thrown due to not being able to send anything other than <see cref="NetworkMessage"/>s to bots.</exception>
	public void QueueSend( string message )
	{
		throw new NotImplementedException();
	}
	
	/// <summary>
	/// Adds a handler for the bot to dispatch the message to.
	/// </summary>
	/// <param name="cb">The method to call when a message of type <see ref="T"/> has come in.</param>
	/// <typeparam name="T">The message type to handle.</typeparam>
	/// <exception cref="Exception">Thrown when a handler has already been set for <see ref="T"/>.</exception>
	public static void HandleBotMessage<T>( Action<BotClient, NetworkMessage> cb ) where T : NetworkMessage
	{
		var messageType = typeof( T );
		if ( MessageHandlers.ContainsKey( messageType ) )
		{
			Log.Error( $"Message type {messageType} is already being handled for bots." );
			return;
		}

		MessageHandlers.Add( messageType, cb );
	}
}
