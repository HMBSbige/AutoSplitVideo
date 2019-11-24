using AutoSplitVideo.Core.SingleInstance;
using AutoSplitVideo.Model;
using AutoSplitVideo.Service;
using AutoSplitVideo.Utils;
using AutoSplitVideo.View;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AutoSplitVideo
{
	public partial class App
	{
		private static int _exited;

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Utils.Utils.GetExecutablePath()));
			if (e.Args.Contains(Constants.ParameterSetAutoRun))
			{
				if (!AutoStartup.Switch())
				{
					Environment.Exit(1);
				}
				Current.Shutdown();
				return;
			}

			var identifier = $@"Global\{UpdateChecker.Name}_{Directory.GetCurrentDirectory().GetDeterministicHashCode()}";
			var singleInstance = new SingleInstance(identifier);
			if (!singleInstance.IsFirstInstance)
			{
				singleInstance.PassArgumentsToFirstInstance(e.Args.Append(Constants.ParameterShow));
				Current.Shutdown();
				return;
			}
			singleInstance.ArgumentsReceived += SingleInstance_ArgumentsReceived;
			singleInstance.ListenForArgumentsFromSuccessiveInstances();

			GlobalConfig.Load();
			Current.DispatcherUnhandledException += (o, args) =>
			{
				if (Interlocked.Increment(ref _exited) == 1)
				{
					MessageBox.Show($@"未捕获异常：{args.Exception}", UpdateChecker.Name, MessageBoxButton.OK, MessageBoxImage.Error);
					Log.ForceLog(args.Exception.ToString());
					GlobalConfig.Save();
					singleInstance.Dispose();
					Current.Shutdown();
				}
			};
			Current.Exit += (o, args) =>
			{
				Utils.Utils.KillFFmpeg();
				singleInstance.Dispose();
				GlobalConfig.Save();
			};

			CheckUpdateAsync();

			MainWindow = new MainWindow();
			if (!e.Args.Contains(Constants.ParameterSilent))
			{
				MainWindow.Show();
			}
		}

		[Conditional("RELEASE")]
		private static async void CheckUpdateAsync()
		{
			var updater = new UpdateChecker();
			updater.NewVersionFound += (o, args) =>
			{
				var msg = $@"发现新版本：{updater.LatestVersionNumber} > {UpdateChecker.Version}";
				Debug.WriteLine(msg);
				var res = MessageBox.Show(
						$@"{msg}{Environment.NewLine}是否跳转到下载页？",
						UpdateChecker.Name, MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (res == MessageBoxResult.Yes)
				{
					Utils.Utils.OpenUrl(updater.LatestVersionUrl);
				}
			};
			updater.NewVersionFoundFailed += (o, args) => { Debug.WriteLine(@"检查更新失败"); };
			updater.NewVersionNotFound += (o, args) =>
			{
				Debug.WriteLine($@"已是最新版本：{UpdateChecker.Version} ≥ {updater.LatestVersionNumber}");
			};
			await updater.Check(true, true);
		}

		private void SingleInstance_ArgumentsReceived(object sender, ArgumentsReceivedEventArgs e)
		{
			if (e.Args.Contains(Constants.ParameterShow))
			{
				Dispatcher?.InvokeAsync(() =>
				{
					MainWindow?.ShowWindow();
				});
			}
		}
	}
}
