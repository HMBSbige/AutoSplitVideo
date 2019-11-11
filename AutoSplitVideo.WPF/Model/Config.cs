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
