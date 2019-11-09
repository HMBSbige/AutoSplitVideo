namespace BilibiliApi.Model
{
	public class UserInfo
	{
		public int code { get; set; }
		public string msg { get; set; }
		public string message { get; set; }
		public UserInfoData data { get; set; }
	}

	public class UserInfoData
	{
		public Info info { get; set; }
	}

	public class Info
	{
		public int uid { get; set; }
		public string uname { get; set; }
	}
}
