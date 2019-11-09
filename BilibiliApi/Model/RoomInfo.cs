namespace BilibiliApi.Model
{
	public class RoomInfo
	{
		public int code { get; set; }
		public string msg { get; set; }
		public string message { get; set; }
		public RoomInfoData data { get; set; }
	}

	public class RoomInfoData
	{
		public int room_id { get; set; }
		public int short_id { get; set; }
		public int live_status { get; set; }
	}
}
