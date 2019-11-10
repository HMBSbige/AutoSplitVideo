using AutoSplitVideo.Model;
using BilibiliApi;
using BilibiliApi.Enum;
using BilibiliApi.Event;
using BilibiliApi.Model;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace AutoSplitVideo.Service
{
	public sealed class StreamMonitor : IDisposable
	{
		public int RoomId { get; }
		public event RoomInfoUpdatedEvent RoomInfoUpdated;
		public event StreamStartedEvent StreamStarted;

		private readonly Timer httpTimer;
		private readonly DanMuClient _danMuClient;

		public StreamMonitor(RoomSetting setting)
		{
			RoomId = setting.RoomId;
			_danMuClient = new DanMuClient(RoomId, TimeSpan.FromMilliseconds(setting.TimingDanmakuRetry));
			_danMuClient.ReceivedDanmaku += (o, args) =>
			{
				switch (args.Danmaku.MsgType)
				{
					case MsgType.LiveStart:
						Task.Run(() => StreamStarted?.Invoke(this, new StreamStartedArgs
						{
							Type = TriggerType.Danmaku,
							IsLive = true
						}));
						break;
					case MsgType.LiveEnd:
						Task.Run(() => StreamStarted?.Invoke(this, new StreamStartedArgs
						{
							Type = TriggerType.Danmaku,
							IsLive = false
						}));
						break;
				}
			};
			httpTimer = new Timer(TimeSpan.FromSeconds(setting.TimingCheckInterval).TotalMilliseconds)
			{
				Enabled = false,
				AutoReset = true,
				SynchronizingObject = null,
				Site = null
			};
			httpTimer.Elapsed += (sender, e) =>
			{
				try
				{
					Check(TriggerType.HttpApi);
				}
				catch
				{
					Debug.WriteLine($@"[{RoomId}] 获取直播间开播状态出错");
				}
			};
		}

		public bool Start()
		{
			if (disposedValue)
			{
				throw new ObjectDisposedException(nameof(StreamMonitor));
			}

			_danMuClient.Start();
			httpTimer.Start();
			return true;
		}

		public void Stop()
		{
			if (disposedValue)
			{
				throw new ObjectDisposedException(nameof(StreamMonitor));
			}

			httpTimer.Stop();
		}

		public async void Check(TriggerType type, int millisecondsDelay = 0)
		{
			if (disposedValue)
			{
				throw new ObjectDisposedException(nameof(StreamMonitor));
			}

			if (millisecondsDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsDelay), @"不能小于0");
			}

			await Task.Delay(millisecondsDelay);
			StreamStarted?.Invoke(this, (await FetchRoomInfoAsync()).IsStreaming
							? new StreamStartedArgs { Type = type, IsLive = true }
							: new StreamStartedArgs { Type = type, IsLive = false });
		}

		public async Task<Room> FetchRoomInfoAsync()
		{
			var room = await BililiveApi.GetRoomInfoAsync(RoomId);
			RoomInfoUpdated?.Invoke(this, new RoomInfoUpdatedArgs { Room = room });
			return room;
		}

		#region IDisposable Support
		private bool disposedValue; // 要检测冗余调用

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					httpTimer?.Dispose();
					_danMuClient?.Dispose();
				}

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