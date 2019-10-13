using AutoSplitVideo.Controller;
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
		private string LiveAddressUrl => $@"https://api.live.bilibili.com/room/v1/Room/playUrl?cid={RealRoomId}&quality=4&platform=web";

		private const string DefaultUserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36";

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

		public async Task<bool> TestHttpOk(string url, int timeout = 3000)
		{
			var client = new HttpClient
			{
				Timeout = TimeSpan.FromMilliseconds(timeout)
			};
			var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			if (response.IsSuccessStatusCode)
			{
				Logging.Info($@"{RealRoomId}:{response.StatusCode.ToString()}");
				return true;
			}
			else
			{
				Logging.Error($@"{RealRoomId}:HTTP Error occurred, the status code is: {response.StatusCode}");
				return false;
			}
		}

		public BilibiliLiveRecorder(long roomId)
		{
			_roomId = roomId;
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add(@"Accept", @"application/json, text/javascript, */*; q=0.01");
			_httpClient.DefaultRequestHeaders.Add(@"Referer", @"https://live.bilibili.com/");
			_httpClient.DefaultRequestHeaders.Add(@"User-Agent", DefaultUserAgent);
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
			foreach (var url in o[@"data"][@"durl"])
			{
				liveUrl.Add(url[@"url"]);
			}
			return liveUrl;
		}
	}
}
