using AutoSplitVideo.MediaInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace AutoSplitVideo.Tests.MediaInfo
{
	[TestClass]
	public class MediaInfoTests
	{
		[TestMethod]
		public void DLLExistTest()
		{
			MediaInfoDll.EnsureDllExists();
			if (!File.Exists(@"MediaInfo.dll"))
			{
				Assert.Fail(@"MediaInfo.dll Not Found");
			}
		}

		[TestMethod]
		public void VersionTest()
		{
			using (var mi = new AutoSplitVideo.MediaInfo.MediaInfo())
			{
				var version = mi.Option(@"Info_Version");
				Debug.WriteLine(version);
				if (version.Length == 0)
				{
					Debug.WriteLine(@"MediaInfo.Dll: this version of the DLL is not compatible");
				}
			}
		}

		[TestMethod]
		public void InfoParametersTest()
		{
			using (var mi = new AutoSplitVideo.MediaInfo.MediaInfo())
			{
				//Information about MediaInfo
				Debug.WriteLine(@"Info_Parameters:");
				Debug.WriteLine(mi.Option(@"Info_Parameters"));
			}
		}

		[TestMethod]
		public void InfoCapacitiesTest()
		{
			using (var mi = new AutoSplitVideo.MediaInfo.MediaInfo())
			{
				Debug.WriteLine(@"Info_Capacities:");
				Debug.WriteLine(mi.Option(@"Info_Capacities"));
			}
		}

		[TestMethod]
		public void InfoCodecsTest()
		{
			using (var mi = new AutoSplitVideo.MediaInfo.MediaInfo())
			{
				Debug.WriteLine(@"Info_Codecs:");
				Debug.WriteLine(mi.Option(@"Info_Codecs"));
			}
		}

		[TestMethod]
		public void GetMediaInfoTest()
		{
			const string fileName = @"D:\Downloads\test.flv";
			using (var mi = new AutoSplitVideo.MediaInfo.MediaInfo())
			{
				mi.Open(fileName);

				Debug.WriteLine(@"Inform with Complete=false");
				mi.Option(@"Complete");
				Debug.WriteLine(mi.Inform());

				Debug.WriteLine(@"Inform with Complete=true");
				mi.Option(@"Complete", @"1");
				Debug.WriteLine(mi.Inform());

				Debug.WriteLine(@"Custom Inform");
				mi.Option(@"Inform", @"General;File size is %FileSize% bytes");
				Debug.WriteLine(mi.Inform());

				Debug.WriteLine(@"Get with Stream=General and Parameter='FileSize'");
				Debug.WriteLine(mi.Get(StreamKind.General, 0, @"FileSize"));

				Debug.WriteLine(@"Get with Stream=General and Parameter=0");
				Debug.WriteLine(mi.Get(StreamKind.General, 0, 0));

				Debug.WriteLine(@"Count_Get with StreamKind=Stream_Audio");
				Debug.WriteLine(mi.Count_Get(StreamKind.Audio));

				Debug.WriteLine(@"Get with Stream=General and Parameter='AudioCount'");
				Debug.WriteLine(mi.Get(StreamKind.General, 0, @"AudioCount"));

				Debug.WriteLine(@"Get with Stream=Audio and Parameter='StreamCount'");
				Debug.WriteLine(mi.Get(StreamKind.Audio, 0, @"StreamCount"));
			}
		}
	}
}