using AutoSplitVideo.Model;
using System;
using System.IO;
using System.Threading;

namespace AutoSplitVideo.Service
{
	public static class TitleLog
	{
		private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
		public const string TitleFileName = @"Title.txt";

		public static void AddLog(RoomSetting room)
		{
			if (!room.LogTitle) return;
			Lock.EnterWriteLock();
			try
			{
				File.AppendAllTextAsync(TitleFileName, $@"[{DateTime.Now}] [{room.RoomId}] [{room.UserName}]：[{room.Title}] {Environment.NewLine}");
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
				File.WriteAllTextAsync(TitleFileName, string.Empty);
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}
	}
}
