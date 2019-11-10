using BilibiliApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnitTest
{
	[TestClass]
	public class BilibiliApiTest
	{
		[TestMethod]
		public async Task GetRoomInfoTest()
		{
			const int roomId = 3;
			var room = await BililiveApi.GetRoomInfoAsync(roomId);
			Assert.IsTrue(room.IsStreaming);
			Assert.AreEqual(room.RoomId, 23058);
			Assert.AreEqual(room.ShortRoomId, 3);
			Assert.AreEqual(room.UserName, @"3号直播间");
			Console.WriteLine(room.Title);
		}

		[TestMethod]
		public async Task GetPlayUrlTest()
		{
			const int roomId = 23058;
			var playUrl = await BililiveApi.GetPlayUrlAsync(roomId);
			Assert.IsTrue(playUrl.StartsWith(@"https://"));
		}

		//[TestMethod]
		public async Task DanMuTest()
		{
			const int roomId = 40462;
			using var client = new DanMuClient(roomId, TimeSpan.FromSeconds(2));
			client.ReceivedDanmaku += (o, args) =>
			{
				var danMu = args.Danmaku;
				Debug.WriteLine(danMu.UserName);
				Debug.WriteLine(danMu.CommentText);
				Debug.WriteLine(danMu.MsgType);
			};
			client.Start();
			await Task.Delay(TimeSpan.FromSeconds(30));
		}
	}
}
