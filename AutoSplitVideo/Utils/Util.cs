using MediaToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
			if (action == null) throw new ArgumentNullException(@"action");

			foreach (var t in collection) action(t);
		}

		public static void KillFFmpeg()
		{
			Process.GetProcessesByName(@"ffmpeg").ForEach(process =>
			{
				try
				{
					if (process.Parent()?.ProcessName == Process.GetCurrentProcess().ProcessName)
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

				Task.Run(() =>
				{
					while (true)
					{
						try
						{
							cts.Token.ThrowIfCancellationRequested();
						}
						catch (OperationCanceledException)
						{
							engine.Dispose();
							return;
						}
						Thread.Sleep(1000);
					}
				}, cts.Token);

				try
				{
					engine.CustomCommand($@"-y -i ""{url}"" -c:v copy -c:a copy ""{path}""");
					cts.Cancel();
				}
				catch
				{
					// ignored
				}

			}, cts.Token);
		}
	}
}
