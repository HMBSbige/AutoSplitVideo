using BilibiliApi.Model;

namespace AutoSplitVideo.Event
{
	public delegate void RoomInfoUpdatedEvent(object sender, RoomInfoUpdatedArgs e);

	public class RoomInfoUpdatedArgs
	{
		public Room Room;
	}
}
