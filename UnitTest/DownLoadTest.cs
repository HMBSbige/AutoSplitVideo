using BilibiliApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UnitTest
{
	[TestClass]
	public class DownLoadTest
	{
		//bytes
		public static long GetFileSize(string sFullName)
		{
			long lSize = 0;
			if (File.Exists(sFullName))
			{
				lSize = new FileInfo(sFullName).Length;
			}

			return lSize;
		}

		[TestMethod]
		public async Task DownloadFileAndCheckSizeTest()
		{
			const string url = @"https://raw.githubusercontent.com/HMBSbige/Text_Translation/master/TrayStatus/ZH-CN.lang";
			const string path = @"D:\Downloads\ZH-CN.lang";

			var instance = new HttpDownLoad(url, path, true);
			await instance.Start();
			instance.Dispose();
			var size = GetFileSize(path);

			Assert.AreEqual(size, 28064);
		}

		[TestMethod]
		public async Task HttpDownLoadTest()
		{
			const int roomId = 23058;
			var playUrl = BililiveApi.GetPlayUrlAsync(roomId).Result;
			Assert.IsTrue(playUrl.StartsWith(@"https://"));

			const string path = @"D:\Downloads\test.flv";

			var instance = new HttpDownLoad(playUrl, path, true);
#pragma warning disable 4014
			Task.Delay(10000).ContinueWith(task =>
#pragma warning restore 4014
			{
				instance.Dispose();
			});
			await instance.Start().ContinueWith(task =>
			{
				Console.WriteLine(task.Status);
				Console.WriteLine(task.IsCanceled);
				Console.WriteLine(task.IsCompleted);
				Console.WriteLine(task.IsCompletedSuccessfully);
				instance.Dispose();
			});
		}
	}
}
