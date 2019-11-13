using AutoSplitVideo.Model;
using System;
using System.IO;
using System.Threading;

namespace AutoSplitVideo.Service
{
	public static class Log
	{
		private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
		public static readonly string LogFileName = $@"{UpdateChecker.Name}.log";

		public static void ForceLog(string str)
		{
			Lock.EnterWriteLock();
			try
			{
				File.AppendAllTextAsync(LogFileName, $@"[{DateTime.Now}] {str}{Environment.NewLine}");
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		public static void AddLog(string str)
		{
			if (!GlobalConfig.Config.LogToFile) return;
			Lock.EnterWriteLock();
			try
			{
				File.AppendAllTextAsync(LogFileName, $@"{str}{Environment.NewLine}");
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		public static void ClearLog()
		{
			Lock.EnterWriteLock();
			try
			{
				File.WriteAllTextAsync(LogFileName, string.Empty);
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}
	}
}
