using BilibiliApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		}

		[TestMethod]
		public async Task GetPlayUrlTest()
		{
			const int roomId = 23058;
			var playUrl = await BililiveApi.GetPlayUrlAsync(roomId);
			Assert.IsTrue(playUrl.StartsWith(@"https://"));
		}
	}
}
