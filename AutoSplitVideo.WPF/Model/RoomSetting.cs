using AutoSplitVideo.ViewModel;
using BilibiliApi.Model;
using System;
using System.Text.Json.Serialization;

namespace AutoSplitVideo.Model
{
	[Serializable]
	public class RoomSetting : ViewModelBase
	{
		#region private

		private int _shortRoomId;
		private int _roomId;
		private string _title;
		private bool _isLive;
		private bool _isRecording;
		private string _userName;
		private uint _timingDanmakuRetry;
		private uint _timingCheckInterval;
		private bool _isMonitor;
		private bool _isNotify;
		private bool _logTitle;

		#endregion

		#region Public

		[JsonIgnore]
		public int ShortRoomId
		{
			get => _shortRoomId;
			set => SetField(ref _shortRoomId, value);
		}

		public int RoomId
		{
			get => _roomId;
			set => SetField(ref _roomId, value);
		}

		[JsonIgnore]
		public string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		[JsonIgnore]
		public bool IsLive
		{
			get => _isLive;
			set => SetField(ref _isLive, value);
		}

		[JsonIgnore]
		public string UserName
		{
			get => _userName;
			set => SetField(ref _userName, value);
		}

		/// <summary>
		/// 弹幕服务器重连时间间隔 毫秒
		/// </summary>
		public uint TimingDanmakuRetry
		{
			get => _timingDanmakuRetry;
			set => SetField(ref _timingDanmakuRetry, value);
		}

		/// <summary>
		/// HTTP API 检查时间间隔 秒
		/// </summary>
		public uint TimingCheckInterval
		{
			get => _timingCheckInterval;
			set => SetField(ref _timingCheckInterval, value);
		}

		[JsonIgnore]
		public bool IsRecording
		{
			get => _isRecording;
			set => SetField(ref _isRecording, value);
		}

		public bool IsMonitor
		{
			get => _isMonitor;
			set => SetField(ref _isMonitor, value);
		}

		public bool IsNotify
		{
			get => _isNotify;
			set => SetField(ref _isNotify, value);
		}

		public bool LogTitle
		{
			get => _logTitle;
			set => SetField(ref _logTitle, value);
		}

		#endregion

		#region Event

		public event EventHandler NotifyEvent;
		public event EventHandler TitleChangedEvent;
		public event EventHandler MonitorChangedEvent;

		#endregion

		public RoomSetting()
		{
			_timingDanmakuRetry = 2000;
			_timingCheckInterval = 300;
			_isMonitor = true;
			_isNotify = true;
			_logTitle = true;

			PropertyChanged += RoomSetting_PropertyChanged;
		}

		private void RoomSetting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(IsLive):
				{
					if (IsLive && IsNotify)
					{
						NotifyEvent?.Invoke(this, new EventArgs());
					}
					break;
				}
				case nameof(Title):
				{
					TitleChangedEvent?.Invoke(this, new EventArgs());
					break;
				}
				case nameof(TimingDanmakuRetry):
				case nameof(TimingCheckInterval):
				{
					MonitorChangedEvent?.Invoke(this, new EventArgs());
					break;
				}
			}
			GlobalConfig.Save();
		}

		public RoomSetting(Room room) : this()
		{
			Parse(room);
		}

		public void Parse(Room room)
		{
			ShortRoomId = room.ShortRoomId;
			RoomId = room.RoomId;
			UserName = room.UserName;
			IsLive = room.IsStreaming;
			Title = room.Title;
		}
	}
}
