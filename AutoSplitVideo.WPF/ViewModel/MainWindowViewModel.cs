using AutoSplitVideo.Core.DataStructure;
using AutoSplitVideo.Model;
using AutoSplitVideo.Service;
using AutoSplitVideo.Utils;
using AutoSplitVideo.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoSplitVideo.ViewModel
{
	public class MainWindowViewModel : ViewModelBase
	{
		private const int MaxLogNum = 100;
		public MainWindowViewModel()
		{
			Window = null;
			_logs = new ObservableQueue<string>(MaxLogNum);
			_progressBarValue = 0;
			_ctsGetDisk = new CancellationTokenSource();
			StartGetDiskUsage(_ctsGetDisk.Token);
			Rooms = new ObservableCollection<RoomSetting>();
			_monitors = new List<StreamMonitor>();
			foreach (var room in CurrentConfig.Rooms)
			{
				var _ = AddRoom(room.RoomId, true);
			}
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

		private ObservableCollection<RoomSetting> _rooms;
		public ObservableCollection<RoomSetting> Rooms
		{
			get => _rooms;
			set => SetField(ref _rooms, value);
		}

		private ObservableQueue<string> _logs;
		public ObservableQueue<string> Logs { get => _logs; set => SetField(ref _logs, value); }

		public static Config CurrentConfig => GlobalConfig.Config;

		public async Task<bool> AddRoom(int roomId, bool isInit = false)
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
				if (!isInit)
				{
					CurrentConfig.Rooms.Add(room);
				}
				StartMonitor(room);
			}
			catch
			{
				return false;
			}
			return true;
		}

		public void RemoveRoom(IEnumerable<int> rooms)
		{
			foreach (var room in rooms)
			{
				StopMonitor(room);

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
		}

		private readonly List<StreamMonitor> _monitors;
		public void StartMonitor(RoomSetting room)
		{
			var monitor = new StreamMonitor(room);
			monitor.RoomInfoUpdated += (o, args) =>
			{
				room.Parse(args.Room);
			};
			monitor.StreamStarted += (o, args) =>
			{
				room.IsLive = args.IsLive;
			};
			monitor.Start();
			_monitors.Add(monitor);
		}

		public void StopMonitor(int roomId)
		{
			var removedMonitors = _monitors.SingleOrDefault(r => r.RoomId == roomId);
			if (removedMonitors != null)
			{
				removedMonitors.Stop();
				_monitors.Remove(removedMonitors);
			}
		}

		public void StopMonitor(IEnumerable<int> rooms)
		{
			foreach (var room in rooms)
			{
				StopMonitor(room);
			}
		}

		public void StopAllMonitors()
		{
			foreach (var monitor in _monitors)
			{
				monitor.Dispose();
			}
		}
	}
}
