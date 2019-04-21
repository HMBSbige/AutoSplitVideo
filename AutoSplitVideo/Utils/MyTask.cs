using AutoSplitVideo.Controller;
using MediaToolkit;
using System.Threading;
using System.Threading.Tasks;

namespace AutoSplitVideo.Utils
{
	public static class MyTask
	{
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
