using FlvProcessor.Event;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FlvProcessor
{
	public class FFmpeg
	{
		public string FFmpegExePath = DefaultFFmpegExePath;

		private const string DefaultFFmpegExePath = @"ffmpeg";

		private Process _process;
		public event EventHandler ProcessExited;
		public event MessageUpdatedEvent MessageUpdated;

		#region Constructor

		public FFmpeg()
		{ }

		public FFmpeg(string path)
		{
			FFmpegExePath = path;
		}

		#endregion

		private static bool Exists(string name)
		{
			try
			{
				var process = new Process
				{
					StartInfo =
						{
								UseShellExecute = false,
								CreateNoWindow = true,
								RedirectStandardInput = true,
								RedirectStandardOutput = true,
								RedirectStandardError = true,
								FileName = name,
								Arguments = @"-version"
						},
					EnableRaisingEvents = true
				};
				process.Start();
				var output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				process.Dispose();
				return output.StartsWith(@"ffmpeg version");
			}
			catch
			{
				return false;
			}
		}

		public void EnsureExists()
		{
			if (Exists(FFmpegExePath))
			{
				return;
			}

			//尝试使用系统的 FFmpeg
			if (Exists(DefaultFFmpegExePath))
			{
				FFmpegExePath = DefaultFFmpegExePath;
				return;
			}

			throw new FileNotFoundException(@"未找到 FFmpeg！", FFmpegExePath);
		}

		public async Task StartAsync(string parameters)
		{
			await Task.Yield();

			EnsureExists();
			Stop(); //单实例单进程
			_process = new Process
			{
				StartInfo =
					{
							UseShellExecute = false,
							CreateNoWindow = true,
							RedirectStandardInput = true,
							RedirectStandardOutput = true,
							RedirectStandardError = true,
							FileName = FFmpegExePath,
							Arguments = parameters
					},
				EnableRaisingEvents = true
			};
			_process.Exited += (o, e) =>
			{
				_process.Dispose();
				ProcessExited?.Invoke(this, new EventArgs());
			};
			_process.Start();

			await ReadOutputAsync(_process.StandardError);
		}

		public void Stop()
		{
			try
			{
				_process?.StandardInput.Write('q');
			}
			catch
			{
				// ignored
			}
			_process?.Dispose();
			_process = null;
		}

		private async Task ReadOutputAsync(TextReader reader)
		{
			await Task.Run(() =>
			{
				string processOutput;
				string lastMessage = null;
				while ((processOutput = reader.ReadLine()) != null)
				{
					lastMessage = processOutput;
					MessageUpdated?.Invoke(this, new MessageUpdatedEventArgs { Message = lastMessage });
				}
				MessageUpdated?.Invoke(this, new MessageUpdatedEventArgs { Message = $@"[已完成]{lastMessage}" });
			});
		}
	}
}
