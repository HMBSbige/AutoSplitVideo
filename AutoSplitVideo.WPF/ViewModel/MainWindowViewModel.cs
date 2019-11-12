using AutoSplitVideo.Core.DataStructure;
using AutoSplitVideo.Model;
using AutoSplitVideo.Service;
using AutoSplitVideo.Utils;
using AutoSplitVideo.View;
using BilibiliApi.Enum;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
			_progressBarValue = 0;
			_ctsGetDisk = new CancellationTokenSource();
			StartGetDiskUsage(_ctsGetDisk.Token);
			_monitors = new List<StreamMonitor>();
			InitRooms(CurrentConfig.Rooms);
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
				StartMonitor(room);
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
				StartMonitor(room);
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
				RemoveMonitor(room);

				var removedRoom = Rooms.SingleOrDefault(r => r.RoomId == room);
				if (removedRoom != null)
				{
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

		private readonly List<StreamMonitor> _monitors;

		private void StartMonitor(RoomSetting room)
		{
			var monitor = new StreamMonitor(room);
			monitor.RoomInfoUpdated += (o, args) => { room.Parse(args.Room); };
			monitor.StreamStarted += (o, args) => { room.IsLive = args.IsLive; };
			monitor.LogEvent += Room_LogEvent;
			monitor.Start();
			AddEvent(room, monitor);
			_monitors.Add(monitor);
		}

		private void RemoveMonitor(int roomId)
		{
			var removedMonitors = _monitors.SingleOrDefault(r => r.RoomId == roomId);
			if (removedMonitors != null)
			{
				removedMonitors.Dispose();
				_monitors.Remove(removedMonitors);
			}
		}

		public void StopAllMonitors()
		{
			foreach (var monitor in _monitors)
			{
				monitor.Dispose();
			}
		}

		public void ManualRefresh(IEnumerable<int> rooms)
		{
			foreach (var roomId in rooms)
			{
				var monitor = _monitors.SingleOrDefault(r => r.RoomId == roomId);
				monitor?.Check(TriggerType.Manual);
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
			room.LogEvent += Room_LogEvent;
		}

		private void Room_LogEvent(object sender, BilibiliApi.Event.LogEventArgs e)
		{
			AddLog(e.Log);
			Debug.WriteLine(e.Log);
		}

		private void AddEvent(RoomSetting room, StreamMonitor monitor)
		{
			room.MonitorChangedEvent += (o, args) =>
			{
				monitor.SettingChanged(room);
			};
		}

		private void Room_NotifyEvent(object sender, EventArgs e)
		{
			if (sender is RoomSetting room)
			{
				Window.Dispatcher?.BeginInvoke(DispatcherPriority.Loaded,
						new Action(() =>
						{
							Window.NotifyIcon.ShowBalloonTip($@"{room.UserName} 开播了！", room.Title,
									BalloonIcon.Info);
						}));
			}
		}

		private void RoomTitleChangedEvent(object sender, EventArgs e)
		{
			if (sender is RoomSetting room)
			{
				TitleLog.AddLog(room);
			}
		}

		#endregion
	}
}
