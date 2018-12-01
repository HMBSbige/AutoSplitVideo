using AutoSplitVideo.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSplitVideo.Tests.Utils
{
	[TestClass]
	public class HttpDownLoadTests
	{
		[TestMethod]
		public void HttpDownLoadTest()
		{
			const string url = @"http://releases.ubuntu.com/18.04.1/ubuntu-18.04.1-desktop-amd64.iso";
			const string path = @"D:\Downloads\ubuntu-18.04.1-desktop-amd64.iso";
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