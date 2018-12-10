using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AutoSplitVideo.MediaInfo;

namespace AutoSplitVideo
{
	static class Program
	{
		private static string ExeName => Assembly.GetExecutingAssembly().GetName().Name;

		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			var isSilent = false;

			foreach (var arg in args)
			{
				if (string.Equals(arg, @"--silent", StringComparison.InvariantCultureIgnoreCase))
				{
					isSilent = true;
				}

				if (string.Equals(arg, @"--setAutoRun", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!Controller.AutoStartup.Switch())
					{
						Environment.ExitCode = 1;
					}

					return;
				}
			}

			using (var mutex = new Mutex(false, $@"Global\{ExeName}_" + Application.StartupPath.GetHashCode()))
			{
				if (!mutex.WaitOne(0, false))
				{
					MessageBox.Show(
						$@"{ExeName} 已经在运行！" + Environment.NewLine +
						$@"请在任务栏里寻找 {ExeName} 图标。" + Environment.NewLine +
						@"如果想启动多份，建议另外复制一份到别的目录。",
						$@"{ExeName} 已经在运行", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				MediaInfoDll.EnsureDllExists();
				var mainForm = new MainForm();
				mainForm.Show();
				if (isSilent)
				{
					mainForm.Hide();
				}

				Application.Run();
			}
		}
	}
}
