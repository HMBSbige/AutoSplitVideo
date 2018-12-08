using AutoSplitVideo.Model;
using AutoSplitVideo.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSplitVideo.Controller;

namespace AutoSplitVideo.Tests.Utils
{
	[TestClass]
	public class HttpDownLoadTests
	{
		[TestMethod]
		public void HttpDownLoadTest()
		{
			var recorder = new BilibiliLiveRecorder(3);

			Assert.ThrowsExceptionAsync<ArgumentException>(async () => { await recorder.GetLiveUrl(); }).Wait();

			recorder.Refresh().Wait();

			var urls = Task.Run(recorder.GetLiveUrl).Result.ToArray();
			var urlNumber = urls.Length;
			Assert.IsTrue(urlNumber > 1);

			var url = urls[0];
			const string path = @"D:\Downloads\test.flv";
			var instance = new HttpDownLoad(url, path, true);
			Task.Run(async () =>
			{
				await instance.Start();
			});
			Thread.Sleep(10000);
			instance.Stop();
		}
	}
}