using AutoSplitVideo.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutoSplitVideo.Model
{
	[Serializable]
	public class Config : ViewModelBase
	{
		#region private

		private string _recordDirectory;
		private List<RoomSetting> _rooms;
		private bool _logToFile;
		private bool _enableAutoConvert;
		private bool _deleteAfterConvert;
		private bool _deleteToRecycle;
		private bool _fixTimestamp;
		private string _token;

		#endregion

		#region Data

		public string RecordDirectory
		{
			get => Directory.Exists(_recordDirectory) ? _recordDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
			set => SetField(ref _recordDirectory, value);
		}

		public List<RoomSetting> Rooms
		{
			get => _rooms;
			set => SetField(ref _rooms, value);
		}

		public bool LogToFile
		{
			get => _logToFile;
			set => SetField(ref _logToFile, value);
		}

		public bool EnableAutoConvert
		{
			get => _enableAutoConvert;
			set => SetField(ref _enableAutoConvert, value);
		}

		public bool DeleteAfterConvert
		{
			get => _deleteAfterConvert;
			set => SetField(ref _deleteAfterConvert, value);
		}

		public bool DeleteToRecycle
		{
			get => _deleteToRecycle;
			set => SetField(ref _deleteToRecycle, value);
		}

		public bool FixTimestamp
		{
			get => _fixTimestamp;
			set => SetField(ref _fixTimestamp, value);
		}

		public string Token
		{
			get => string.IsNullOrEmpty(_token) || _token.Length != 32 ? string.Empty : _token;
			set => SetField(ref _token, value);
		}

		#endregion

		public Config()
		{
			_rooms = new List<RoomSetting>();
			_recordDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
			LogToFile = true;
			PropertyChanged += (o, args) => { GlobalConfig.Save(); };
		}
	}
}
