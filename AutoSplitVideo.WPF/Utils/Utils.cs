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

		public static bool OpenUrl(string path)
		{
			try
			{
				new Process
				{
					StartInfo = new ProcessStartInfo(path)
					{
						UseShellExecute = true
					}
				}.Start();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool OpenDir(string dir)
		{
			if (Directory.Exists(dir))
			{
				try
				{
					return OpenUrl(dir);
				}
				catch
				{
					// ignored
				}
			}
			return false;
		}

		public static bool OpenFile(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					return OpenUrl(path);
				}
				catch
				{
					// ignored
				}
			}
			return false;
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

		public static string CountSize(long size)
		{
			var mStrSize = string.Empty;
			const double step = 1024.00;
			var factSize = size;
			if (factSize < step)
			{
				mStrSize = $@"{factSize:F2} Byte";
			}
			else if (factSize >= step && factSize < 1048576)
			{
				mStrSize = $@"{factSize / step:F2} KB";
			}
			else if (factSize >= 1048576 && factSize < 1073741824)
			{
				mStrSize = $@"{factSize / step / step:F2} MB";
			}
			else if (factSize >= 1073741824 && factSize < 1099511627776)
			{
				mStrSize = $@"{factSize / step / step / step:F2} GB";
			}
			else if (factSize >= 1099511627776)
			{
				mStrSize = $@"{factSize / step / step / step / step:F2} TB";
			}

			return mStrSize;
		}

		public static (long, long) GetDiskUsage(string path)
		{
			try
			{
				var allDrives = DriveInfo.GetDrives();
				foreach (var d in allDrives)
				{
					if (d.Name == Path.GetPathRoot(path))
					{
						if (d.IsReady)
						{
							return (d.AvailableFreeSpace, d.TotalSize);
						}
					}
				}
				return (0, 0);
			}
			catch
			{
				return (0, 0);
			}
		}
	}
}
