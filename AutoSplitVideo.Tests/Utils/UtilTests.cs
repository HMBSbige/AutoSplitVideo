using AutoSplitVideo.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
	}
}