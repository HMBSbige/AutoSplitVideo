using AutoSplitVideo.MediaInfo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

		private static string FindIndexedProcessName(int pid)
		{
			var processName = Process.GetProcessById(pid).ProcessName;
			var processesByName = Process.GetProcessesByName(processName);
			string processIndexName = null;

			for (var index = 0; index < processesByName.Length; ++index)
			{
				processIndexName = index == 0 ? processName : $@"{processName}#{index}";
				var processId = new PerformanceCounter(@"Process", @"ID Process", processIndexName);
				if (Convert.ToInt32(processId.NextValue()) == pid)
				{
					return processIndexName;
				}
			}

			return processIndexName;
		}

		private static Process FindPidFromIndexedProcessName(string indexedProcessName)
		{
			var parentId = new PerformanceCounter(@"Process", @"Creating Process ID", indexedProcessName);
			return Process.GetProcessById((int)parentId.NextValue());
		}

		private static Process Parent(this Process process)
		{
			return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
		}

		private static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			foreach (var t in collection) action(t);
		}

		public static void KillFFmpeg()
		{
			Process.GetProcessesByName(@"ffmpeg").ForEach(process =>
			{
				try
				{
					if (process.Parent()?.Id == Process.GetCurrentProcess().Id)
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

		public static IEnumerable<long> ToListInt(this string str)
		{
			var s = str.Split(',');
			var res = new List<long>();
			foreach (var longS in s)
			{
				if (long.TryParse(longS, out var l))
				{
					res.Add(l);
				}
			}
			return res;
		}

		public static string ToStr(this IEnumerable<long> list)
		{
			return string.Join(@",", list.Select(l => Convert.ToString(l)));
		}

		public static RegistryKey OpenRegKey(string name, bool writable, RegistryHive hive = RegistryHive.CurrentUser)
		{
			var userKey = RegistryKey.OpenBaseKey(hive, Environment.Is64BitProcess ? RegistryView.Registry64 : RegistryView.Registry32).OpenSubKey(name, writable);
			return userKey;
		}

		public static int RunAsAdmin(string arguments)
		{
			Process process;
			var processInfo = new ProcessStartInfo
			{
				Verb = "runas",
				FileName = Application.ExecutablePath,
				Arguments = arguments
			};
			try
			{
				process = Process.Start(processInfo);
			}
			catch (System.ComponentModel.Win32Exception)
			{
				return -1;
			}

			process?.WaitForExit();

			var ret = process.ExitCode;
			process.Close();
			return ret;
		}

		public static bool IsFlv(string path)
		{
			using (var mi = new MediaInfo.MediaInfo())
			{
				mi.Open(path);
				var format = mi.Get(StreamKind.General, 0, @"Format");
				return string.Equals(format, @"Flash Video", StringComparison.Ordinal);
			}
		}

		public static bool IsMp4(string path)
		{
			using (var mi = new MediaInfo.MediaInfo())
			{
				mi.Open(path);
				var format = mi.Get(StreamKind.General, 0, @"Format");
				return string.Equals(format, @"MPEG-4", StringComparison.Ordinal);
			}
		}

		public static List<string> GetAllFlvList(string dir)
		{
			var list = Directory.GetFiles(dir, @"*.flv", SearchOption.TopDirectoryOnly);
			var res = new List<string>();
			using (var mi = new MediaInfo.MediaInfo())
			{
				foreach (var p in list)
				{
					var path = Path.GetFullPath(p);
					mi.Open(path);
					var format = mi.Get(StreamKind.General, 0, @"Format");
					if (string.Equals(format, @"Flash Video", StringComparison.Ordinal))
					{
						res.Add(path);
					}
				}
			}
			return res;
		}
	}
}
