using AutoSplitVideo.Model;
using System;
using System.IO;

namespace AutoSplitVideo.Service
{
	public static class Log
	{
		private static readonly string LogFileName = $@"{UpdateChecker.Name}.log";

		public static void AddLog(string str)
		{
			if (GlobalConfig.Config.LogToFile)
			{
				File.AppendAllTextAsync(LogFileName, $@"{str}{Environment.NewLine}");
			}
		}

		public static void ClearLog()
		{
			File.WriteAllTextAsync(LogFileName, string.Empty);
		}
	}
}
