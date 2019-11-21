namespace FlvProcessor.Event
{
	public delegate void MessageUpdatedEvent(object sender, MessageUpdatedEventArgs e);

	public class MessageUpdatedEventArgs
	{
		public string Message;
	}
}