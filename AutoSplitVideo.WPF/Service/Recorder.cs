using AutoSplitVideo.Model;
using AutoSplitVideo.ViewModel;
using BilibiliApi;
using BilibiliApi.Enum;
using BilibiliApi.Event;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AutoSplitVideo.Service
{
	public sealed class Recorder : ViewModelBase, IDisposable
	{
		private string DirName => Path.Combine(GlobalConfig.Config.RecordDirectory, $@"{_currentRoom.RoomId}");
		private string FileName => Path.Combine(DirName, $@"{DateTime.Now:yyyyMMdd_HHmmss}.flv");

		private readonly RoomSetting _currentRoom;
		public event LogEvent LogEvent;

		private HttpResponseMessage _response;
		private HttpDownLoad _downLoadTask;

		public Recorder(RoomSetting setting)
		{
			_currentRoom = setting;
		}

		public async void Start()
		{
			while (true)
			{
				if (_disposedValue)
				{
					return;
				}

				if (_currentRoom.IsRecording == RecordingStatus.Recording)
				{
					LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_currentRoom.RoomId}] 已经在录制中了" });
					return;
				}

				_currentRoom.IsRecording = RecordingStatus.Starting;

				EnsureDirectory();

				if (!_currentRoom.IsLive) return;

				var url = await BililiveApi.GetPlayUrlAsync(_currentRoom.RoomId);
				url = await GetRedirectUrl(url);

				if (_response.StatusCode != HttpStatusCode.OK)
				{
					LogEvent?.Invoke(this, new LogEventArgs { Log = $@"尝试下载直播流时服务器返回了 ({_response.StatusCode}){_response.ReasonPhrase}" });

					await ReCheck();
					continue;
				}

				_downLoadTask = new HttpDownLoad(url, FileName, true);
				try
				{
					_currentRoom.IsRecording = RecordingStatus.Recording;
					await _downLoadTask.Start(_response)
							.ContinueWith(task =>
							{
								LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_currentRoom.RoomId}] 录制结束" });
							});
				}
				catch (Exception ex)
				{
					LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_currentRoom.RoomId}] 录制出错：{ex}" });
				}
				finally
				{
					Stop();
					await ReCheck();
					Start();
				}

				break;
			}
		}

		private void EnsureDirectory()
		{
			if (!Directory.Exists(DirName))
			{
				var dirInfo = Directory.CreateDirectory(DirName);
				if (!dirInfo.Exists)
				{
					LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_currentRoom.RoomId}] 存储目录创建失败" });
				}
			}
		}

		private async Task<string> GetRedirectUrl(string url)
		{
			while (true)
			{
				using (var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate, }))
				{
					client.Timeout = TimeSpan.FromMilliseconds(_currentRoom.TimingStreamConnect);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"*/*"));
					client.DefaultRequestHeaders.UserAgent.Clear();
					client.DefaultRequestHeaders.UserAgent.ParseAdd(BilibiliApi.Utils.UserAgent);
					client.DefaultRequestHeaders.Referrer = new Uri(@"https://live.bilibili.com");
					client.DefaultRequestHeaders.Add(@"Origin", @"https://live.bilibili.com");

					LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_currentRoom.RoomId}] 连接直播服务器 {new Uri(url).Host}" });
					Debug.WriteLine($@"[{_currentRoom.RoomId}] 直播流地址: {url}");

					_response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
				}

				LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_currentRoom.RoomId}] HTTP status code: {_response.StatusCode.ToString()}" });

				if (_response.StatusCode == HttpStatusCode.Redirect || _response.StatusCode == HttpStatusCode.Moved)
				{
					// workaround for missing Referrer
					url = _response.Headers.Location.OriginalString;
					LogEvent?.Invoke(this, new LogEventArgs { Log = $@"[{_currentRoom.RoomId}] Redirect to {new Uri(url).Host}" });
					_response.Dispose();
					continue;
				}

				return url;
			}
		}

		private async Task ReCheck()
		{
			if (_disposedValue)
			{
				return;
			}

			_currentRoom.Monitor.Check(TriggerType.HttpApiRecheck);
			await Task.Delay((int)_currentRoom.TimingStreamRetry + 1000);
		}

		private void Stop()
		{
			if (_disposedValue)
			{
				return;
			}

			_currentRoom.IsRecording = RecordingStatus.Stopped;
			_response?.Dispose();
			_downLoadTask?.Dispose();
		}

		#region IDisposable Support
		private bool _disposedValue; // 要检测冗余调用

		private void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Stop();
				}

				_response = null;
				_downLoadTask = null;

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
