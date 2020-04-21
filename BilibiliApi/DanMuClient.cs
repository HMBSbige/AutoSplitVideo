using BilibiliApi.Event;
using BilibiliApi.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliApi
{
	public sealed class DanMuClient : IDisposable
	{
		private readonly int _roomId;
		private TimeSpan _timingDanmakuRetry;
		private TcpClient _client;
		private const string DmServerHost = @"broadcastlv.chat.bilibili.com";
		private const int DmServerPort = 2243;
		private NetworkStream _dmNetStream;
		private readonly CancellationTokenSource _cts;

		private bool DmTcpConnected => _client?.Connected ?? false;

		public event ReceivedDanmakuEvent ReceivedDanmaku;
		public event LogEvent LogEvent;

		public DanMuClient(int roomId, TimeSpan timingDanmakuRetry)
		{
			_roomId = roomId;
			_timingDanmakuRetry = timingDanmakuRetry;
			_cts = new CancellationTokenSource();
			HeartBeatTask(_cts.Token);
		}

		public void SetTimingDanmakuRetry(TimeSpan timingDanmakuRetry)
		{
			_timingDanmakuRetry = timingDanmakuRetry;
		}

		public void Start()
		{
			ConnectWithRetry();
		}

		private void HeartBeatTask(CancellationToken token)
		{
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					if (_dmNetStream != null && _dmNetStream.CanWrite)
					{
						try
						{
							SendSocketData(2);
						}
						catch
						{
							// ignored
						}
					}
					if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30)))
					{
						break;
					}
				}
			}, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		private async void ConnectWithRetry()
		{
			var connectResult = false;
			while (!DmTcpConnected && !_cts.Token.IsCancellationRequested)
			{
				await Task.Delay(_timingDanmakuRetry);
				LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 正在连接弹幕服务器..." });
				connectResult = Connect();
			}

			if (connectResult)
			{
				LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 弹幕服务器连接成功" });
			}
		}

		private bool Connect()
		{
			if (DmTcpConnected)
			{
				return true;
			}

			try
			{
				_client = new TcpClient();
				_client.Connect(DmServerHost, DmServerPort);
				_dmNetStream = _client.GetStream();
				Task.Run(ReceiveMessageLoop);

				SendSocketData(7, $@"{{""roomid"":{_roomId},""uid"":0}}");
				SendSocketData(2);

				return true;
			}
			catch
			{
				LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 连接弹幕服务器错误" });
				return false;
			}
		}

		private void ReceiveMessageLoop()
		{
			try
			{
				var headerBuff = new byte[16];
				while (DmTcpConnected)
				{
					_dmNetStream.ReadB(headerBuff, 0, 16);
					var protocol = new DanmuProtocol(headerBuff);

					if (protocol.PacketLength < 16)
					{
						throw new NotSupportedException($@"协议失败: (L:{protocol.PacketLength})");
					}
					var bodyLength = protocol.PacketLength - 16;
					if (bodyLength == 0)
					{
						continue; //没有内容了
					}
					var buffer = new byte[bodyLength];
					_dmNetStream.ReadB(buffer, 0, bodyLength);
					switch (protocol.Version)
					{
						case 0:
						case 1:
						{
							ProcessDanmu(protocol.Operation, buffer, bodyLength);
							break;
						}
						case 2:
						{
							using var ms = new MemoryStream(buffer, 2, bodyLength - 2);
							using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
							while (deflate.Read(headerBuff, 0, 16) > 0)
							{
								protocol = new DanmuProtocol(headerBuff);
								bodyLength = protocol.PacketLength - 16;
								if (bodyLength == 0)
								{
									continue; // 没有内容了
								}
								if (buffer.Length < bodyLength) // 不够长再申请
								{
									buffer = new byte[bodyLength];
								}
								deflate.Read(buffer, 0, bodyLength);
								ProcessDanmu(protocol.Operation, buffer, bodyLength);
							}
							break;
						}
						default:
						{
							LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 弹幕协议不支持" });
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				_client?.Close();
				_dmNetStream = null;
				if (!(_cts?.IsCancellationRequested ?? true))
				{
					LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 弹幕连接被断开，将尝试重连" });
					ConnectWithRetry();
				}
			}
		}

		private void ProcessDanmu(int opt, byte[] buffer, int length)
		{
			switch (opt)
			{
				case 5:
				{
					var json = string.Empty;
					try
					{
						json = Encoding.UTF8.GetString(buffer, 0, length);
						ReceivedDanmaku?.Invoke(this, new ReceivedDanmakuArgs { Danmaku = new Danmaku(json) });
					}
					catch (Exception ex)
					{
						if (ex is JsonException || ex is KeyNotFoundException)
						{
							LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] 弹幕识别错误 {json}" });
						}
						else
						{
							LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_roomId}] {ex}" });
						}
					}
					break;
				}
			}
		}

		private void SendSocketData(int action, string body = @"")
		{
			const int param = 1;
			const short magic = 16;
			const short ver = 1;

			var playLoad = Encoding.UTF8.GetBytes(body);
			var buffer = new byte[playLoad.Length + 16];

			using var ms = new MemoryStream(buffer);
			var b = BitConverter.GetBytes(buffer.Length).ToBE();
			ms.Write(b, 0, 4);
			b = BitConverter.GetBytes(magic).ToBE();
			ms.Write(b, 0, 2);
			b = BitConverter.GetBytes(ver).ToBE();
			ms.Write(b, 0, 2);
			b = BitConverter.GetBytes(action).ToBE();
			ms.Write(b, 0, 4);
			b = BitConverter.GetBytes(param).ToBE();
			ms.Write(b, 0, 4);
			if (playLoad.Length > 0)
			{
				ms.Write(playLoad, 0, playLoad.Length);
			}
			_dmNetStream.Write(buffer, 0, buffer.Length);
			_dmNetStream.Flush();
		}

		#region IDisposable Support
		private bool _disposedValue; // 要检测冗余调用
		private readonly object _lock = new object();

		private void Dispose(bool disposing)
		{
			lock (_lock)
			{
				if (_disposedValue)
				{
					return;
				}
				_disposedValue = true;
			}
			if (disposing)
			{
				_cts?.Cancel();
				_cts?.Dispose();
				_client?.Close();
				_client?.Dispose();
				_dmNetStream?.Dispose();
			}
			_dmNetStream = null;
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
