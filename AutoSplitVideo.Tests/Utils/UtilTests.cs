using AutoSplitVideo.Controller;
using AutoSplitVideo.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AutoSplitVideo.Tests.Utils
{
	[TestClass]
	public class UtilTests
	{
		[TestMethod]
		public void DownloadFileAndCheckSizeTest()
		{
			const string url = @"https://raw.githubusercontent.com/HMBSbige/Text_Translation/master/TrayStatus/ZH-CN.lang";
			const string path = @"D:\Downloads\ZH-CN.lang";

			var instance = new HttpDownLoad(url, path, true);
			instance.Start().Wait();

			var size = Util.GetFileSize(path);

			Assert.AreEqual(size, 28064);
		}

		[TestMethod]
		public void DiskUsageTest()
		{
			const string path = @"D:\Downloads\ZH-CN.lang";
			var (availableFreeSpace, totalSize) = Util.GetDiskUsage(path);

			Console.WriteLine(Util.CountSize(availableFreeSpace));

			Console.WriteLine($@"{Util.CountSize(totalSize - availableFreeSpace)}/{Util.CountSize(totalSize)}");
		}
	}
}