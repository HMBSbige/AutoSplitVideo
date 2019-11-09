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

		#endregion

		#region Data

		public List<long> RoomList { get; set; }

		public string RecordDirectory
		{
			get => Directory.Exists(_recordDirectory) ? _recordDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
			set => SetField(ref _recordDirectory, value);
		}

		#endregion

		public Config()
		{
			RoomList = new List<long>();
			RecordDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
		}
	}
}
