using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutoSplitVideo
{
	public class BilibiliLiveRecorder
	{
		private string RoomInfoUrl => $@"https://api.live.bilibili.com/room/v1/Room/get_info?room_id={_roomId}";
		private string UserInfoUrl => $@"https://api.live.bilibili.com/live_user/v1/UserInfo/get_anchor_in_room?roomid={RealRoomId}";
		private string LiveAddressUrl => $@"https://api.live.bilibili.com/api/playurl?cid={RealRoomId}&otype=json&quality=0&platform=web";

		private readonly long _roomId;
		public long RealRoomId = 0;
		public string Title;
		public bool IsLive;

		private static async Task<string> Get(string uri)
		{
			var httpClient = new HttpClient();
			var response = await httpClient.GetAsync(uri);
			response.EnsureSuccessStatusCode();
			var resultStr = await response.Content.ReadAsStringAsync();

			Debug.WriteLine(resultStr);
			return resultStr;
		}

		public BilibiliLiveRecorder(int roomId)
		{
			_roomId = roomId;
		}

		public async Task Refresh()
		{
			var jsonStr = await Get(RoomInfoUrl);
			dynamic o = SimpleJson.SimpleJson.DeserializeObject(jsonStr);
			RealRoomId = o[@"data"][@"room_id"];
			Title = o[@"data"][@"title"];
			IsLive = o[@"data"][@"live_status"] == 1;
		}

		public async Task<string> GetAnchorName()
		{
			if (RealRoomId == 0)
			{
				throw new ArgumentException(@"RealRoomId Wrong!");
			}
			var jsonStr = await Get(UserInfoUrl);
			dynamic o = SimpleJson.SimpleJson.DeserializeObject(jsonStr);
			return o[@"data"][@"info"][@"uname"];
		}

		public async Task<IEnumerable<string>> GetLiveUrl()
		{
			if (RealRoomId == 0)
			{
				throw new ArgumentException(@"RealRoomId Wrong!");
			}
			var jsonStr = await Get(LiveAddressUrl);
			dynamic o = SimpleJson.SimpleJson.DeserializeObject(jsonStr);
			var liveUrl = new List<string>();
			foreach (var url in o[@"durl"])
			{
				liveUrl.Add(url[@"url"]);
			}
			return liveUrl;
		}
	}
}
