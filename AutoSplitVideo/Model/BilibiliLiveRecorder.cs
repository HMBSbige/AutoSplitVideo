using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutoSplitVideo.Model
{
	public class BilibiliLiveRecorder
	{
		private string RoomInfoUrl => $@"https://api.live.bilibili.com/room/v1/Room/get_info?room_id={_roomId}";
		private string UserInfoUrl => $@"https://api.live.bilibili.com/live_user/v1/UserInfo/get_anchor_in_room?roomid={RealRoomId}";
		private string LiveAddressUrl => $@"https://api.live.bilibili.com/api/playurl?cid={RealRoomId}&otype=json&quality=0&platform=web";

		private HttpClient _httpClient;

		private readonly long _roomId;
		public long RealRoomId = 0;
		public string Title;
		public bool IsLive;
		public string Msg;
		public string Message;

		private async Task<string> GetAsync(string uri)
		{
			var response = await _httpClient.GetAsync(uri);
			response.EnsureSuccessStatusCode();
			var resultStr = await response.Content.ReadAsStringAsync();

			Debug.WriteLine(resultStr);
			return resultStr;
		}

		public async Task<bool> TestHttpOk(string url)
		{
			var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

			if (response.IsSuccessStatusCode)
			{
				Debug.WriteLine(response.StatusCode.ToString());
				return true;
			}
			else
			{
				Debug.WriteLine($@"HTTP Error occurred, the status code is: {response.StatusCode}");
				return false;
			}

		}

		public BilibiliLiveRecorder(long roomId)
		{
			_roomId = roomId;
			_httpClient = new HttpClient();
		}

		public async Task Refresh()
		{
			Message = $@"房间 {RealRoomId} 信息获取失败";
			var jsonStr = await GetAsync(RoomInfoUrl);
			dynamic o = SimpleJson.SimpleJson.DeserializeObject(jsonStr);
			Msg = o[@"msg"];
			Message = o[@"message"];
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
			var jsonStr = await GetAsync(UserInfoUrl);
			dynamic o = SimpleJson.SimpleJson.DeserializeObject(jsonStr);
			return o[@"data"][@"info"][@"uname"];
		}

		public async Task<IEnumerable<string>> GetLiveUrl()
		{
			if (RealRoomId == 0)
			{
				throw new ArgumentException(@"RealRoomId Wrong!");
			}
			_httpClient = new HttpClient();
			var jsonStr = await GetAsync(LiveAddressUrl);
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
