using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AutoSplitVideo.Utils
{
	public static class Utils
	{
		public static int GetDeterministicHashCode(this string str)
		{
			unchecked
			{
				var hash1 = (5381 << 16) + 5381;
				var hash2 = hash1;

				for (var i = 0; i < str.Length; i += 2)
				{
					hash1 = ((hash1 << 5) + hash1) ^ str[i];
					if (i == str.Length - 1)
						break;
					hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
				}

				return hash1 + hash2 * 1566083941;
			}
		}

		public static void OpenUrl(string path)
		{
			new Process
			{
				StartInfo = new ProcessStartInfo(path)
				{
					UseShellExecute = true
				}
			}.Start();
		}

		public static string GetDllPath()
		{
			return Assembly.GetExecutingAssembly().Location;
		}

		public static string GetExecutablePath()
		{
			var p = Process.GetCurrentProcess();
			if (p.MainModule != null)
			{
				var res = p.MainModule.FileName;
				return res;
			}

			var dllPath = GetDllPath();
			return Path.Combine(Path.GetDirectoryName(dllPath) ?? throw new InvalidOperationException(), $@"{Path.GetFileNameWithoutExtension(dllPath)}.exe");
		}
	}
}
