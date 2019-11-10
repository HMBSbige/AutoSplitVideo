using BilibiliApi.Model;

namespace BilibiliApi.Event
{
	public delegate void RoomInfoUpdatedEvent(object sender, RoomInfoUpdatedArgs e);

	public class RoomInfoUpdatedArgs
	{
		public Room Room;
	}
}
