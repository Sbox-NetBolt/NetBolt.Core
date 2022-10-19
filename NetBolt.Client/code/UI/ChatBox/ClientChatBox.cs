using NetBolt.Shared.Messages;
using Sandbox.UI.Construct;
using Sandbox;
using Sandbox.UI;
using NetBolt.Shared.Clients;

namespace NetBolt.Client.UI;

/// <summary>
/// A simple chat box.
/// </summary>
public class ClientChatBox : Panel
{
	/// <summary>
	/// The only instance of <see cref="ClientChatBox"/> existing.
	/// </summary>
	public static ClientChatBox? Current;

	/// <summary>
	/// The canvas that contains the chat.
	/// </summary>
	public Panel Canvas { get; }
	/// <summary>
	/// The text entry for input.
	/// </summary>
	public TextEntry Input { get; }

	/// <summary>
	///  Initializes a new instance of <see cref="ClientChatBox"/>.
	/// </summary>
	public ClientChatBox()
	{
		Current = this;

		StyleSheet.Load( "/UI/ChatBox/ClientChatBox.scss" );

		Canvas = Add.Panel( "chat_canvas" );

		Input = Add.TextEntry( "" );
		Input.AddEventListener( "onsubmit", Submit );
		Input.AddEventListener( "onblur", Close );
		Input.AcceptsFocus = true;
		Input.AllowEmojiReplace = true;
	}

	/// <summary>
	/// Adds a new chat entry to the chat box.
	/// </summary>
	/// <param name="name">The name of the person that created the message.</param>
	/// <param name="message">The message.</param>
	/// <param name="avatar">The avatar in the "avatar:steamid64" format.</param>
	public void AddEntry( string? name, string message, string? avatar )
	{
		var entry = Canvas.AddChild<ChatEntry>();

		entry.Message.Text = message;
		entry.NameLabel.Text = name;
		entry.Avatar.SetTexture( avatar );

		entry.SetClass( "noname", string.IsNullOrEmpty( name ) );
		entry.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );
	}

	/// <summary>
	/// Opens the chat box.
	/// </summary>
	private void Open()
	{
		AddClass( "open" );
		Input.Focus();
	}

	/// <summary>
	/// Closes the chat box.
	/// </summary>
	private void Close()
	{
		RemoveClass( "open" );
		Input.Blur();
	}

	/// <summary>
	/// Submits the current message to the chat box.
	/// </summary>
	private void Submit()
	{
		Close();

		var msg = Input.Text.Trim();
		Input.Text = "";

		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		Say( msg );
	}

	/// <summary>
	/// Checks if the chat box should be opened.
	/// </summary>
	/// <param name="inputBuilder">The current input builder.</param>
	[Event.BuildInput]
	private static void BuildInput( InputBuilder inputBuilder )
	{
		if ( inputBuilder.Pressed( InputButton.Chat ) )
			OpenChat();
	}

	/// <summary>
	/// Adds a new entry to the chat box.
	/// </summary>
	/// <param name="name">The name of the person that create the message.</param>
	/// <param name="message">The message.</param>
	/// <param name="avatar">The avatar in the "avatar:steamid64" format.</param>
	public static void AddChatEntry( string name, string message, string? avatar = null )
	{
		Current?.AddEntry( name, message, avatar );
	}

	/// <summary>
	/// Adds information to the chat box.
	/// </summary>
	/// <param name="message">The information.</param>
	/// <param name="avatar">The avatar in the "avatar:steamid64" format.</param>
	public static void AddInformation( string message, string? avatar = null )
	{
		Current?.AddEntry( null, message, avatar );
	}

	/// <summary>
	/// Opens the chat box.
	/// </summary>
	public static void OpenChat()
	{
		Current?.Open();
	}

	/// <summary>
	/// Says a message from the local client.
	/// </summary>
	/// <param name="message">The message the local client typed.</param>
	private static void Say( string message )
	{
		// TODO: Reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		NetworkManager.Instance?.SendToServer( new ClientSayMessage( INetworkClient.Local, message ) );
		AddChatEntry( Local.Client.Name, message, $"avatar:{Local.PlayerId}" );
	}
}
