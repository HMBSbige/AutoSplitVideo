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
			_httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
			_httpClient.DefaultRequestHeaders.Add("Referer", "https://live.bilibili.com/");
			_httpClient.DefaultRequestHeaders.Add("User-Agent", Utils.UserAgent);
		}

		public static async Task ApplyCookieSettings(string cookieString)
		{
			await SemaphoreSlim.WaitAsync();
			try
			{
				if (!string.IsNullOrWhiteSpace(cookieString))
				{
					try
					{
						var cc = new CookieContainer { PerDomainCapacity = 300 };
						foreach (var t in cookieString.Trim(' ', ';').Split(';').Select(x => x.Trim().Split(new[] { '=' }, 2)))
						{
							try
							{
								var v = string.Empty;
								if (t.Length == 2)
								{
									v = System.Web.HttpUtility.UrlDecode(t[1])?.Trim();
								}

								cc.Add(new Cookie(t[0].Trim(), v, "/", ".bilibili.com"));
							}
							catch (Exception)
							{
								// ignored
							}
						}

						var pclient = new HttpClient(new HttpClientHandler
						{
							CookieContainer = cc
						}, true)
						{
							Timeout = TimeSpan.FromSeconds(5)
						};
						pclient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
						pclient.DefaultRequestHeaders.Add("Referer", "https://live.bilibili.com/");
						pclient.DefaultRequestHeaders.Add("User-Agent", Utils.UserAgent);

						_httpClient = pclient;
						return;
					}
					catch
					{
						Debug.WriteLine("设置 Cookie 时发生错误");
					}
				}

				var cleanclient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
				cleanclient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
				cleanclient.DefaultRequestHeaders.Add("Referer", "https://live.bilibili.com/");
				cleanclient.DefaultRequestHeaders.Add("User-Agent", Utils.UserAgent);
				_httpClient = cleanclient;
			}
			finally
			{
				SemaphoreSlim.Release();
			}
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
			var playUrl = JsonSerializer.Deserialize<PlayUrl>(json);
			var dUrls = playUrl.data.durl;
			{
				var urls = dUrls.Select(dUrl => dUrl.url).Distinct().ToArray();
				if (urls.Length > 0)
				{
					return urls[Random.Next(0, urls.Length - 1)];
				}
			}
			throw new Exception("没有直播播放地址");
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
				var roomJson = await GetAsync($@"https://api.live.bilibili.com/room/v1/Room/get_info?id={roomId}");
				var room = JsonSerializer.Deserialize<RoomInfo>(roomJson);
				if (room.code != 0)
				{
					Debug.WriteLine($"不能获取 {roomId} 的信息1: {room.message}");
					return null;
				}

				var userJson = await GetAsync($@"https://api.live.bilibili.com/live_user/v1/UserInfo/get_anchor_in_room?roomid={roomId}");
				var user = JsonSerializer.Deserialize<UserInfo>(userJson);
				if (user.code != 0)
				{
					Debug.WriteLine($"不能获取 {roomId} 的信息2: {room.message}");
					return null;
				}

				var i = new Room
				{
					ShortRoomId = room.data.short_id,
					RoomId = room.data.room_id,
					IsStreaming = 1 == room.data.live_status,
					UserName = user.data.info.uname
				};
				return i;
			}
			catch
			{
				Debug.WriteLine($"获取直播间 {roomId} 的信息时出错");
				throw;
			}
		}
	}
}
