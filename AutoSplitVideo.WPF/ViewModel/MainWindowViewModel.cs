using AutoSplitVideo.Core.DataStructure;
using AutoSplitVideo.Model;
using AutoSplitVideo.Service;
using AutoSplitVideo.Utils;
using AutoSplitVideo.View;
using BilibiliApi.Event;
using BilibiliApi.Model;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AutoSplitVideo.ViewModel
{
	public class MainWindowViewModel : ViewModelBase
	{
		public static Config CurrentConfig => GlobalConfig.Config;

		private const int MaxLogNum = 1000;

		public MainWindowViewModel()
		{
			Window = null;
			_logs = new ObservableQueue<string>(MaxLogNum);
			_videoConverter = new ObservableCollection<VideoConvert>();
			_progressBarValue = 0;
			_ctsGetDisk = new CancellationTokenSource();
			StartGetDiskUsage(_ctsGetDisk.Token);

			Token = CurrentConfig.Token;
			ApplyToApi().ContinueWith(task =>
			{
				InitRooms(CurrentConfig.Rooms);
			});
		}

		#region Window

		public MainWindow Window { private get; set; }

		public void HideWindow()
		{
			if (Window == null)
			{
				return;
			}
			Window.Visibility = Visibility.Hidden;
		}

		public void ShowWindow(bool notClosing = true)
		{
			Window?.ShowWindow(notClosing);
		}

		public void TriggerShowHide()
		{
			if (Window == null)
			{
				return;
			}

			if (Window.Visibility == Visibility.Visible)
			{
				HideWindow();
			}
			else
			{
				ShowWindow();
			}
		}

		#endregion

		#region ProgressBar

		private double _progressBarValue;
		public double ProgressBarValue
		{
			get => _progressBarValue;
			set => SetField(ref _progressBarValue, value);
		}

		private string _progressBarStr;
		public string ProgressBarStr
		{
			get => _progressBarStr;
			set => SetField(ref _progressBarStr, value);
		}

		#endregion

		#region GetDiskUsage

		private CancellationTokenSource _ctsGetDisk;
		private static readonly TimeSpan GetDiskInterval = TimeSpan.FromSeconds(1);
		private async void StartGetDiskUsage(CancellationToken ct)
		{
			while (true)
			{
				var (availableFreeSpace, totalSize) = Utils.Utils.GetDiskUsage(CurrentConfig.RecordDirectory);
				if (totalSize != 0)
				{
					ProgressBarStr = $@"已使用 {Utils.Utils.CountSize(totalSize - availableFreeSpace)}/{Utils.Utils.CountSize(totalSize)} 剩余 {Utils.Utils.CountSize(availableFreeSpace)}";
					var percentage = (totalSize - availableFreeSpace) / (double)totalSize;
					ProgressBarValue = percentage * 100;
				}
				else
				{
					ProgressBarStr = string.Empty;
					ProgressBarValue = 0;
				}
				try
				{
					await Task.Delay(GetDiskInterval, ct);
				}
				catch (TaskCanceledException)
				{
					break;
				}
				if (ct.IsCancellationRequested)
				{
					break;
				}
			}
		}

		public void StopGetDiskUsage()
		{
			_ctsGetDisk.Cancel();
			_ctsGetDisk = new CancellationTokenSource();
		}

		#endregion

		#region Log

		private ObservableQueue<string> _logs;

		public ObservableQueue<string> Logs
		{
			get => _logs;
			set => SetField(ref _logs, value);
		}

		public void AddLog(string str)
		{
			var log = $@"[{DateTime.Now}] {str}";
			Logs.Enqueue(log);
			Log.AddLog(log);
		}

		#endregion

		#region Room

		private ObservableCollection<RoomSetting> _rooms;
		public ObservableCollection<RoomSetting> Rooms
		{
			get => _rooms;
			set => SetField(ref _rooms, value);
		}

		private void InitRooms(IEnumerable<RoomSetting> rooms)
		{
			Rooms = new ObservableCollection<RoomSetting>(rooms);
			foreach (var room in Rooms)
			{
				AddEvent(room);
				room.StartMonitor();
			}
		}

		public async Task<bool> AddRoom(int roomId)
		{
			if (Rooms.Any(roomSetting => roomId == roomSetting.RoomId || roomId == roomSetting.ShortRoomId))
			{
				return false;
			}

			try
			{
				var roomInfo = await BilibiliApi.BililiveApi.GetRoomInfoAsync(roomId);
				var room = new RoomSetting(roomInfo);
				Rooms.Add(room);
				CurrentConfig.Rooms.Add(room);
				AddEvent(room);
				room.StartMonitor();
			}
			catch
			{
				return false;
			}
			finally
			{
				GlobalConfig.Save();
			}
			return true;
		}

		public void RemoveRoom(IEnumerable<int> rooms)
		{
			foreach (var room in rooms)
			{
				var removedRoom = Rooms.SingleOrDefault(r => r.RoomId == room);
				if (removedRoom != null)
				{
					removedRoom.StopMonitor();
					removedRoom.StopRecorder();
					Rooms.Remove(removedRoom);
				}

				var removedRoom2 = CurrentConfig.Rooms.SingleOrDefault(r => r.RoomId == room);
				if (removedRoom2 != null)
				{
					CurrentConfig.Rooms.Remove(removedRoom2);
				}
			}
			GlobalConfig.Save();
		}

		#endregion

		#region Monitor

		public void StopAllMonitors()
		{
			foreach (var room in Rooms)
			{
				room.StopMonitor();
				room.StopRecorder();
			}
		}

		public void ManualRefresh(IEnumerable<int> rooms)
		{
			foreach (var roomId in rooms)
			{
				var room = Rooms.SingleOrDefault(r => r.RoomId == roomId);
				room?.Monitor?.Check(TriggerType.手动);
			}
		}

		#endregion

		#region Event

		private void AddEvent(RoomSetting room)
		{
			room.NotifyEvent -= Room_NotifyEvent;
			room.NotifyEvent += Room_NotifyEvent;
			room.TitleChangedEvent -= RoomTitleChangedEvent;
			room.TitleChangedEvent += RoomTitleChangedEvent;
			room.LogEvent -= Room_LogEvent;
			room.LogEvent += Room_LogEvent;
			room.RecordCompletedEvent -= Room_RecordCompletedEvent;
			room.RecordCompletedEvent += Room_RecordCompletedEvent;
		}

		private void Room_LogEvent(object sender, LogEventArgs e)
		{
			AddLog(e.Log);
			Debug.WriteLine(e.Log);
		}

		private void Room_NotifyEvent(object sender, EventArgs e)
		{
			if (sender is RoomSetting room)
			{
				Window?.Dispatcher?.InvokeAsync(() =>
				{
					Window.NotifyIcon.ShowBalloonTip($@"{room.UserName} 开播了！", room.Title, BalloonIcon.Info);
				}, DispatcherPriority.Loaded);
			}
		}

		private void RoomTitleChangedEvent(object sender, EventArgs e)
		{
			if (sender is RoomSetting room)
			{
				TitleLog.AddLog(room);
			}
		}

		private void Room_RecordCompletedEvent(object sender, LogEventArgs e)
		{
			var filePath = e.Log;
			if (File.Exists(filePath) && CurrentConfig.EnableAutoConvert)
			{
				Window.Dispatcher?.InvokeAsync(() =>
				{
					AddConvertTask(filePath, Path.ChangeExtension(filePath, @"mp4"),
					CurrentConfig.DeleteAfterConvert, CurrentConfig.DeleteToRecycle, CurrentConfig.FixTimestamp);
				});
			}
		}

		#endregion

		#region VideoConvert

		private ObservableCollection<VideoConvert> _videoConverter;

		public ObservableCollection<VideoConvert> VideoConverter
		{
			get => _videoConverter;
			set => SetField(ref _videoConverter, value);
		}

		public async void AddConvertTask(string inputPath, string outputPath, bool isDelete, bool deleteToRecycle, bool fixTimestamp)
		{
			try
			{
				var videoConvert = new VideoConvert();
				VideoConverter.Add(videoConvert);
				await videoConvert.Convert(inputPath, outputPath, isDelete, deleteToRecycle, fixTimestamp);
			}
			catch (Exception ex)
			{
				AddLog($@"[Error] {ex}");
			}
		}

		public void AddSplitTask(string inputPath, string outputPath, string startTime, string duration)
		{
			try
			{
				var videoConvert = new VideoConvert();
				VideoConverter.Add(videoConvert);
				videoConvert.Split(inputPath, outputPath, startTime, duration);
			}
			catch (Exception ex)
			{
				AddLog($@"[Error] {ex}");
			}
		}

		public void StopAllVideoConvert()
		{
			foreach (var videoConvert in VideoConverter)
			{
				videoConvert.Stop();
			}
		}

		#endregion

		#region 登录

		private string _account;
		private string _password;
		private string _token;
		private string _status;
		private string _refreshToken;
		private string _cookie;
		private string _csrf;
		private string _uid;

		public string Account
		{
			get => _account;
			set => SetField(ref _account, value);
		}

		public string Password
		{
			get => _password;
			set => SetField(ref _password, value);
		}

		public string Token
		{
			get => _token;
			set => SetField(ref _token, value);
		}

		public string Status
		{
			get => string.IsNullOrWhiteSpace(_status) ? @"未知" : _status;
			set => SetField(ref _status, value);
		}

		public string RefreshToken
		{
			get => _refreshToken;
			set => SetField(ref _refreshToken, value);
		}

		public string Cookie
		{
			get => _cookie;
			set
			{
				SetField(ref _cookie, value);
				var b = new BilibiliCookie(_cookie);
				Csrf = b.bili_jct;
				Uid = b.DedeUserID;
			}
		}

		public string Csrf
		{
			get => _csrf;
			set => SetField(ref _csrf, value);
		}

		public string Uid
		{
			get => _uid;
			set => SetField(ref _uid, value);
		}

		private void UpdateStatus(string str)
		{
			Status = str;
			AddLog(str);
		}

		public void ParseCookie(Dictionary<string, string> cookies)
		{
			var b = new BilibiliCookie(cookies);
			Cookie = b.ToString();
			UpdateStatus(@"获取 Cookie 成功");
		}

		public async Task GetToken()
		{
			try
			{
				var token = await BilibiliApi.BililiveApi.LoginAsync(Account, Password);
				if (token == null)
				{
					UpdateStatus(@"获取 Access Token 失败");
				}
				else
				{
					Token = token.AccessToken;
					RefreshToken = token.RefreshToken;
					Cookie = token.Cookie.ToString();
					UpdateStatus($@"获取 Access Token 成功，Token 有效期至 {token.Expires.AddHours(8)}");
					Account = Password = string.Empty;
				}
			}
			catch (Exception ex)
			{
				UpdateStatus($@"获取 Access Token 失败，{ex.Message}");
			}
		}

		public async Task ApplyToApi()
		{
			try
			{
				if (string.IsNullOrEmpty(Cookie) && string.IsNullOrEmpty(Token))
				{
					BilibiliApi.BililiveApi.Reload(null);
					UpdateStatus(@"未登录/注销成功");
					return;
				}

				if (!string.IsNullOrEmpty(Cookie))
				{
					BilibiliApi.BililiveApi.Reload(Cookie);
					if (await BilibiliApi.BililiveApi.CheckLoginStatus())
					{
						UpdateStatus(@"Cookie 登录成功");
						return;
					}

					UpdateStatus(@"Cookie 登录失败");
					BilibiliApi.BililiveApi.Reload(null);
				}

				if (!string.IsNullOrEmpty(Cookie))
				{
					if (BilibiliApi.Utils.IsToken(Token))
					{
						try
						{
							var tokenInfo = await BilibiliApi.BililiveApi.GetTokenInfo(Token);
							if (tokenInfo == null)
							{
								UpdateStatus(@"Token 登录失败，Token 错误");
							}
							else
							{
								BilibiliApi.BililiveApi.Reload($@"SESSDATA={tokenInfo.AccessToken}");
								UpdateStatus($@"Token 登录成功，Token 有效期至 {tokenInfo.Expires.AddHours(8)}");
							}
						}
						catch (Exception ex)
						{
							UpdateStatus($@"Token 登录失败，{ex.Message}");
						}
					}
					else
					{
						UpdateStatus(@"Token 登录失败，Access Token 格式错误");
					}
				}
			}
			finally
			{
				CurrentConfig.Token = Token;
				CurrentConfig.RefreshToken = RefreshToken;
				CurrentConfig.Cookie = Cookie;
			}
		}

		public async Task Revoke()
		{
			if (!string.IsNullOrEmpty(Cookie))
			{
				if (await BilibiliApi.BililiveApi.Logout())
				{
					LogoutSucceed();
					return;
				}
				UpdateStatus(@"Cookie 注销失败");
			}

			if (BilibiliApi.Utils.IsToken(Token))
			{
				if (await BilibiliApi.BililiveApi.RevokeToken(Token))
				{
					LogoutSucceed();
					return;
				}
				UpdateStatus(@"Token 注销失败");
			}
			else
			{
				UpdateStatus(@"注销失败，Access Token 格式错误");
			}
		}

		private void LogoutSucceed()
		{
			Token = string.Empty;
			RefreshToken = string.Empty;
			Cookie = string.Empty;
			UpdateStatus(@"注销成功");
			BilibiliApi.BililiveApi.Reload(null);
		}

		#endregion
	}
}
