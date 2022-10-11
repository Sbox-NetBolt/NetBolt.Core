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
	
	public void QueueSend( NetworkMessage message )
	{
		if ( !MessageHandlers.TryGetValue( message.GetType(), out var cb ) )
		{
			Log.Error( $"Unhandled message type {message.GetType()} for bot." );
			return;
		}

		cb.Invoke( this, message );
	}

	public Task DisconnectAsync( WebSocketDisconnectReason reason = WebSocketDisconnectReason.Requested, string strReason = "",
		WebSocketError? error = null )
	{
		throw new NotImplementedException();
	}

	public Task HandleAsync()
	{
		return Task.CompletedTask;
	}

	public ValueTask<int> PingAsync( int timeout = int.MaxValue )
	{
		return new ValueTask<int>( 0 );
	}

	public void QueueSend( byte[] bytes )
	{
		throw new NotImplementedException();
	}

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
