using BilibiliApi.Model;

namespace BilibiliApi.Event
{
	public delegate void ReceivedDanmakuEvent(object sender, ReceivedDanmakuArgs e);

	public class ReceivedDanmakuArgs
	{
		public Danmaku Danmaku;
	}
}
