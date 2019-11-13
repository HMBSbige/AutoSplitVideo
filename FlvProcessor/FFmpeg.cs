using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FlvProcessor
{
	public class FFmpeg
	{
		public string FFmpegExePath = @"ffmpeg.exe";

		private const string DefaultFFmpegExePath = @"ffmpeg.exe";

		private Process _process;
		public event EventHandler ProcessExited;

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
			if (Exists(DefaultFFmpegExePath))
			{
				FFmpegExePath = DefaultFFmpegExePath; //默认使用系统的 FFmpeg
				return;
			}
			if (!Exists(FFmpegExePath))
			{
				throw new FileNotFoundException(@"未找到 FFmpeg！", FFmpegExePath);
			}
		}

		public void StartAsync(string parameters)
		{
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

			ReadOutputAsync(_process.StandardError);
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

		private static async void ReadOutputAsync(TextReader reader)
		{
			await Task.Run(() =>
			{
				string processOutput;
				while ((processOutput = reader.ReadLine()) != null)
				{
					//TODO
					Console.WriteLine(processOutput);
				}
			});
		}
	}
}
