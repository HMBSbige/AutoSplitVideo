using AutoSplitVideo.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AutoSplitVideo.ViewModel
{
	public class Rooms : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = @"")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public Rooms(long roomId)
		{
			_recorder = new BilibiliLiveRecorder(roomId);
			_anchorName = string.Empty;
		}

		#region Private

		private readonly BilibiliLiveRecorder _recorder;
		private string _anchorName;
		private bool _isRecording;

		#endregion

		#region Public

		public long RealRoomID => _recorder.RealRoomId;
		public string Title => _recorder.Title;
		public bool IsLive => _recorder.IsLive;
		public string LiveStatus => _recorder.IsLive ? @"直播中..." : @"闲置";
		public string Message => _recorder.Message;

		public string AnchorName
		{
			get => _anchorName;
			set
			{
				if (value != _anchorName)
				{
					_anchorName = value;
					NotifyPropertyChanged();
				}
			}
		}

		public bool IsRecording
		{
			get => _isRecording;
			set
			{
				if (value != _isRecording)
				{
					_isRecording = value;
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(RecordingStatus));
				}
			}
		}

		public string RecordingStatus => IsRecording ? @"录制中..." : @"等待开播";

		#endregion

		public async Task Refresh()
		{
			await _recorder.Refresh();
			NotifyPropertyChanged(nameof(RealRoomID));
			NotifyPropertyChanged(nameof(Title));
			NotifyPropertyChanged(nameof(IsLive));
			NotifyPropertyChanged(nameof(LiveStatus));
			AnchorName = await _recorder.GetAnchorName();
		}

		public async Task<IEnumerable<string>> GetLiveUrl()
		{
			return await _recorder.GetLiveUrl();
		}
	}
}
