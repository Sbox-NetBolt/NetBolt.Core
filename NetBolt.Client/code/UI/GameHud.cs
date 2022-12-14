using System.Text;
using NetBolt.Shared.Entities;
using NetBolt.Shared.Messages;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace NetBolt.Client;

/// <summary>
/// The root panel for HUD elements.
/// </summary>
[UseTemplate]
public class GameHud : RootPanel
{
	/// <summary>
	/// A property for the connection info of the server.
	/// </summary>
	public string ConnectionInfo
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return "Connected: No";

			return $"Connected: {NetworkManager.Instance.Address}:{NetworkManager.Instance.Port}";
		}
	}

	/// <summary>
	/// A property for the amount of network updates that are happening per second.
	/// </summary>
	public string TickRate
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return string.Empty;

			return $"{NetworkManager.Instance.TickRate} tps";
		}
	}

	/// <summary>
	/// A property for the amount of clients in the server that the client knows about.
	/// </summary>
	public string ClientCount
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return string.Empty;

			return $"{NetworkManager.Instance?.Clients.Count} clients connected";
		}
	}

	/// <summary>
	/// A property for the amount of entities in the server that the client knows about.
	/// </summary>
	public string NetworkedEntityCount
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return string.Empty;

			return $"{IEntity.All.Count} networked entities";
		}
	}

#if DEBUG
	/// <summary>
	/// A debug property for the amount of messages that have been received from the server.
	/// </summary>
	public string MessagesReceived
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return string.Empty;

			return $"{NetworkManager.Instance?.MessagesReceived} messages received";
		}
	}

	/// <summary>
	/// A debug property for the amount of messages that have been sent to the server.
	/// </summary>
	public string MessagesSent
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return string.Empty;

			return $"{NetworkManager.Instance?.MessagesSent} messages sent";
		}
	}

	/// <summary>
	/// A debug property to display all of the <see cref="NetworkMessage"/> types that have been sent and how many of them.
	/// </summary>
	public string MessageTypesSent
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return string.Empty;

			var sb = new StringBuilder();
			sb.Append( "Sent Types:\n" );

			foreach ( var pair in NetworkManager.Instance.MessageTypesSent )
			{
				sb.Append( pair.Key.Name );
				sb.Append( ": " );
				sb.Append( pair.Value );
				sb.Append( '\n' );
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// A debug property to display all of the <see cref="NetworkMessage"/> types that have been received and how many of them.
	/// </summary>
	public string MessageTypesReceived
	{
		get
		{
			if ( NetworkManager.Instance is null || !NetworkManager.Instance.Connected )
				return string.Empty;

			var sb = new StringBuilder();
			sb.Append( "Received Types:\n" );

			foreach ( var pair in NetworkManager.Instance.MessageTypesReceived )
			{
				sb.Append( pair.Key.Name );
				sb.Append( ": " );
				sb.Append( pair.Value );
				sb.Append( '\n' );
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// Initializes a new instance of <see cref="GameHud"/>.
	/// </summary>
	public GameHud()
	{
		var messagesReceivedLabel = Add.Label( MessagesReceived, "debugLabel netReceivedNum" );
		var messagesSentLabel = Add.Label( MessagesSent, "debugLabel netSentNum" );
		var messageTypesSentLabel = Add.Label( MessageTypesSent, "debugLabel netTypeSentNum" );
		var messageTypesReceivedLabel = Add.Label( MessageTypesReceived, "debugLabel netTypeReceivedNum" );
		messagesReceivedLabel.Bind( "text", this, nameof( MessagesReceived ) );
		messagesSentLabel.Bind( "text", this, nameof( MessagesSent ) );
		messageTypesSentLabel.Bind( "text", this, nameof( MessageTypesSent ) );
		messageTypesReceivedLabel.Bind( "text", this, nameof( MessageTypesReceived ) );
	}
#endif
}
