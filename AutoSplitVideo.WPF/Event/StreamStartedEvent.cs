using AutoSplitVideo.Model;

namespace AutoSplitVideo.Event
{
	public delegate void StreamStartedEvent(object sender, StreamStartedArgs e);

	public class StreamStartedArgs
	{
		public TriggerType Type;
		public bool IsLive;
	}
}
