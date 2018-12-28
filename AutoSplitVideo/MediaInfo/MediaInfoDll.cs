using AutoSplitVideo.Properties;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace AutoSplitVideo.MediaInfo
{
	public static class MediaInfoDll
	{
		private static readonly string DllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MediaInfo.dll");

		public static void EnsureDllExists()
		{
			if (!File.Exists(DllPath))
			{
				UnpackDll(DllPath);
			}
		}

		private static void UnpackDll(string path)
		{
			var compressedFFmpegStream = Assembly.GetExecutingAssembly()
					.GetManifestResourceStream(Environment.Is64BitProcess
							? Resources.MediaInfo64ManifestResourceName
							: Resources.MediaInfoManifestResourceName);


			if (compressedFFmpegStream == null)
			{
				throw new Exception(@"MediaInfo.dll ERROR");
			}

			using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				using (var compressedStream = new GZipStream(compressedFFmpegStream, CompressionMode.Decompress))
				{
					compressedStream.CopyTo(fileStream);
				}
			}
		}
	}
}
