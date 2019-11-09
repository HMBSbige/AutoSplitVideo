namespace BilibiliApi.Model
{
	public class PlayUrl
	{
		public int code { get; set; }
		public string message { get; set; }
		public PlayUrlData data { get; set; }
	}

	public class PlayUrlData
	{
		public Durl[] durl { get; set; }
	}

	public class Durl
	{
		public string url { get; set; }
	}
}
