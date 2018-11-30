using System.IO;

namespace AutoSplitVideo.Utils
{
	public static class Util
	{
		public static long GetFileSize(string sFullName)
		{
			long lSize = 0;
			if (File.Exists(sFullName))
			{
				lSize = new FileInfo(sFullName).Length;
			}

			return lSize;
		}
	}
}
