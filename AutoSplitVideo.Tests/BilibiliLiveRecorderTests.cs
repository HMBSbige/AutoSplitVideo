using AutoSplitVideo.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSplitVideo.Tests
{
	[TestClass]
	public class BilibiliLiveRecorderTests
	{
		[TestMethod]
		public void RefreshTest()
		{
			var recorder = new BilibiliLiveRecorder(115);
			recorder.Refresh().Wait();

			Assert.AreEqual(recorder.RealRoomId, 1016);
			Assert.IsNotNull(recorder.Title);
			Assert.IsNotNull(recorder.IsLive);
		}

		[TestMethod]
		public void GetAnchorNameTest()
		{
			var recorder = new BilibiliLiveRecorder(115);

			Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			{
				await recorder.GetAnchorName();
			}).Wait();

			recorder.Refresh().Wait();

			var name = Task.Run(recorder.GetAnchorName).Result;
			Assert.AreEqual(name, @"神奇陆夫人");
		}

		[TestMethod]
		public void GetLiveUrlTest()
		{
			var recorder = new BilibiliLiveRecorder(115);

			Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			{
				await recorder.GetLiveUrl();
			}).Wait();

			recorder.Refresh().Wait();

			var urls = Task.Run(recorder.GetLiveUrl).Result.ToArray();
			var urlNumber = urls.Length;
			Assert.IsTrue(urlNumber > 1);
		}
	}
}