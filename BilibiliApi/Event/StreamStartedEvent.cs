using BilibiliApi.Enum;

namespace BilibiliApi.Event
{
	public delegate void StreamStartedEvent(object sender, StreamStartedArgs e);

	public class StreamStartedArgs
	{
		public TriggerType Type;
		public bool IsLive;
	}
}
