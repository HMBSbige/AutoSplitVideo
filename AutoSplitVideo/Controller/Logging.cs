using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AutoSplitVideo.Controller
{
	public static class Logging
	{
		private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AutoSplitVideo.log");

		public static void Error(object o)
		{
			var str = $@"[{DateTime.Now}] ERROR {o}{Environment.NewLine}";
			Debug.Write(str);
			UpdateLogFile(str);
		}

		public static void Info(object o)
		{
			var str = $@"[{DateTime.Now}] INFO {o}{Environment.NewLine}";
			Debug.Write(str);
			UpdateLogFile(str);
		}

		private static void UpdateLogFile(string str)
		{
			File.AppendAllText(LogFile, str, Encoding.UTF8);
		}
	}
}
