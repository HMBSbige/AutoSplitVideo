using AutoSplitVideo.Controller;
using MediaToolkit;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

		public static async Task FFmpegRecordTask(string url, string path, CancellationTokenSource cts)
		{
			await Task.Run(() =>
			{
				var engine = new Engine();
				var ctsEndTask = new CancellationTokenSource();

				cts.Token.Register(() =>
				{
					if (!ctsEndTask.IsCancellationRequested)
					{
						ctsEndTask.Cancel();
					}
				});
				ctsEndTask.Token.Register(() => { engine.Dispose(); });

				try
				{
					engine.CustomCommand($@"-y -i ""{url}"" -c:v copy -c:a copy ""{path}""");
					ctsEndTask.Cancel();
				}
				catch
				{
					// ignored
				}

			}, cts.Token);
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
			var userKey = RegistryKey.OpenBaseKey(hive, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)
					.OpenSubKey(name, writable);
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

		public static async Task HttpDownLoadRecordTask(string url, string path, CancellationTokenSource cts)
		{
			var instance = new HttpDownLoad(url, path, true);
			var ctsEndTask = new CancellationTokenSource();

			cts.Token.Register(() =>
			{
				if (!ctsEndTask.IsCancellationRequested)
				{
					ctsEndTask.Cancel();
				}
			});
			ctsEndTask.Token.Register(() => { instance.Stop(); });

			try
			{
				await instance.Start();
				ctsEndTask.Cancel();
			}
			catch
			{
				// ignored
			}
		}
	}
}
