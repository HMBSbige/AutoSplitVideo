using BilibiliApi.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliApi
{
	public static class BililiveApi
	{
		private static readonly Random Random = new Random();
		private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
		private static HttpClient _httpClient;

		static BililiveApi()
		{
			Reload(null);
		}

		public static void Reload(string cookie)
		{
			if (string.IsNullOrEmpty(cookie))
			{
				_httpClient = new HttpClient();
			}
			else
			{
				_httpClient = new HttpClient(new HttpClientHandler { UseCookies = false, UseDefaultCredentials = false }, true);
				_httpClient.DefaultRequestHeaders.Add(@"Cookie", cookie);
			}

			_httpClient.DefaultRequestVersion = new Version(2, 0);
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
			_httpClient.DefaultRequestHeaders.Add(@"Accept", @"application/json, text/javascript, */*; q=0.01");
			_httpClient.DefaultRequestHeaders.Add(@"Referer", @"https://live.bilibili.com/");
			_httpClient.DefaultRequestHeaders.Add(@"User-Agent", Utils.UserAgent);
		}

		public static async Task<string> GetAsync(string url)
		{
			await SemaphoreSlim.WaitAsync();
			try
			{
				var str = await _httpClient.GetStringAsync(url);
				Debug.WriteLine(str);
				return str;
			}
			finally
			{
				SemaphoreSlim.Release();
			}
		}

		/// <summary>
		/// 获取直播间播放地址
		/// </summary>
		/// <param name="roomId">原房间号</param>
		/// <returns>FLV播放地址</returns>
		/// <exception cref="WebException"/>
		/// <exception cref="Exception"/>
		public static async Task<string> GetPlayUrlAsync(int roomId)
		{
			var url = $@"https://api.live.bilibili.com/room/v1/Room/playUrl?cid={roomId}&quality=4&platform=web";
			var json = await GetAsync(url);
			using var document = JsonDocument.Parse(json);
			var root = document.RootElement;
			if (root.TryGetProperty(@"data", out var data)
			&& data.TryGetProperty(@"durl", out var durls)
			&& durls.ValueKind == JsonValueKind.Array)
			{
				var urls = durls.EnumerateArray().Select(dUrl => dUrl.GetProperty(@"url").GetString()).Distinct().ToArray();
#if DEBUG
				foreach (var s in urls)
				{
					Debug.WriteLine(s);
				}
#endif
				if (urls.Length > 0)
				{
					return urls[Random.Next(urls.Length)];
				}
			}
			throw new Exception(@"获取直播间播放地址失败");
		}

		/// <summary>
		/// 获取直播间信息
		/// </summary>
		/// <param name="roomId">房间号（允许短号）</param>
		/// <returns>直播间信息</returns>
		/// <exception cref="WebException"/>
		/// <exception cref="Exception"/>
		public static async Task<Room> GetRoomInfoAsync(int roomId)
		{
			try
			{
				var res = new Room();
				var roomJson = await GetAsync($@"https://api.live.bilibili.com/room/v1/Room/get_info?id={roomId}");
				using var document = JsonDocument.Parse(roomJson);
				var room = document.RootElement;
				if (room.TryGetProperty(@"code", out var code) && code.GetInt32() != 0)
				{
					Debug.WriteLine($@"不能获取 {roomId} 的信息1: {room.GetProperty(@"message").GetString()}");
					return null;
				}

				res.Title = room.GetProperty(@"data").GetProperty(@"title").GetString();
				res.ShortRoomId = room.GetProperty(@"data").GetProperty(@"short_id").GetInt32();
				res.RoomId = room.GetProperty(@"data").GetProperty(@"room_id").GetInt32();
				res.IsStreaming = room.GetProperty(@"data").GetProperty(@"live_status").GetInt32() == 1;

				var userJson = await GetAsync($@"https://api.live.bilibili.com/live_user/v1/UserInfo/get_anchor_in_room?roomid={roomId}");
				using var document2 = JsonDocument.Parse(userJson);
				var user = document2.RootElement;
				if (user.TryGetProperty(@"code", out var code2) && code2.GetInt32() != 0)
				{
					Debug.WriteLine($@"不能获取 {roomId} 的信息2: {user.GetProperty(@"message").GetString()}");
					return null;
				}

				res.UserName = user.GetProperty(@"data").GetProperty(@"info").GetProperty(@"uname").GetString();
				return res;
			}
			catch
			{
				Debug.WriteLine($@"获取直播间 {roomId} 的信息时出错");
				throw;
			}
		}

		public static async Task<BilibiliTokenv3> LoginAsync(string userName, string password)
		{
			var logKey = new LogInKey(await Passport.Passport.GetHash());
			if (logKey.Code == 0)
			{
				var token = new BilibiliTokenv3(await Passport.Passport.Login(logKey.Hash, logKey.Key, userName, password));
				if (token.Code == 0)
				{
					return token;
				}
			}
			return null;
		}

		public static async Task<BilibiliToken> GetTokenInfo(string accessToken)
		{
			var token = new BilibiliToken(await Passport.Passport.GetTokenInfo(accessToken));
			return token.Code == 0 ? token : null;
		}

		public static async Task<bool> RevokeToken(string accessToken)
		{
			try
			{
				var json = await Passport.Passport.Revoke(accessToken);
				using var document = JsonDocument.Parse(json);
				var root = document.RootElement;
				return root.TryGetProperty(@"code", out var code) && code.GetInt32() == 0;
			}
			catch
			{
				return false;
			}
		}

		public static async Task<bool> CheckLoginStatus()
		{
			try
			{
				var json = await GetAsync(@"https://api.bilibili.com/x/space/myinfo");
				using var document = JsonDocument.Parse(json);
				var root = document.RootElement;
				return root.TryGetProperty(@"code", out var code) && code.GetInt32() == 0;
			}
			catch
			{
				return false;
			}
		}

		public static async Task<bool> Logout()
		{
			try
			{
				var res = await _httpClient.GetAsync(@"https://passport.bilibili.com/login?act=exit");
				return res.StatusCode == HttpStatusCode.OK;
			}
			catch
			{
				return false;
			}
		}
	}
}
