using AutoSplitVideo.Model;
using System;

namespace AutoSplitVideo.Service
{
	public class Recorder : IDisposable
	{
		public Recorder(RoomSetting setting)
		{

		}

		public void Start()
		{

		}

		public void Stop()
		{

		}

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)。
					Stop();
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
