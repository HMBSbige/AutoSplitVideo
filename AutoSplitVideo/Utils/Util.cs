using System;
using MediaToolkit.Util;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

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

		public static string SelectPath() //弹出一个选择目录的对话框
		{
			var path = new FolderBrowserDialog();
			path.ShowDialog();
			return path.SelectedPath;
		}

		public static void StopFFmpeg()
		{
			Process.GetProcessesByName(@"ffmpeg").ForEach(process =>
			{
				try
				{
					if (Path.GetDirectoryName(process.MainModule.FileName) == Environment.CurrentDirectory)
					{
						process.Kill();
						process.WaitForExit();
					}
				}
				catch
				{
					// ignored
				}
			});
		}
	}
}
