using AutoSplitVideo.Utils;
using AutoSplitVideo.ViewModel;
using FlvProcessor;
using System.IO;

namespace AutoSplitVideo.Model
{
	public class VideoConvert : ViewModelBase
	{
		#region Private

		private string _currentStatus;
		private string _description;

		#endregion

		#region Public

		public string CurrentStatus
		{
			get => _currentStatus;
			set
			{
				lock (this)
				{
					SetField(ref _currentStatus, value);
				}
			}
		}

		public string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		#endregion

		private FFmpeg _ffmpeg;

		public VideoConvert()
		{
			_currentStatus = @"未开始";
			_description = @"未知";
			_ffmpeg = null;
		}

		public void Convert(string inputPath, string outputPath, bool isDelete, bool deleteToRecycle)
		{
			var parameters = $@"-i ""{inputPath}"" -c copy ""{outputPath}"" -y";
			Stop();
			_ffmpeg = new FFmpeg();
			_ffmpeg.MessageUpdated += FFmpeg_MessageUpdated;
			_ffmpeg.ProcessExited += (o, args) =>
			{
				if (isDelete)
				{
					if (deleteToRecycle)
					{
						Win32.MoveToRecycleBin(inputPath);
					}
					else
					{
						File.Delete(inputPath);
					}
				}
			};
			Description = $@"转封装：""{inputPath}"" => ""{outputPath}""";
			_ffmpeg.StartAsync(parameters);
		}

		public void Split(string inputPath, string outputPath, string startTime, string duration)
		{
			var parameters = $@"-ss {startTime} -t {duration} -accurate_seek -i ""{inputPath}"" -codec copy -avoid_negative_ts 1 ""{outputPath}""";
			Stop();
			_ffmpeg = new FFmpeg();
			_ffmpeg.MessageUpdated += FFmpeg_MessageUpdated;
			Description = $@"手动剪辑：""{inputPath}"" => ""{outputPath}""";
			_ffmpeg.StartAsync(parameters);
		}

		private void FFmpeg_MessageUpdated(object sender, FlvProcessor.Event.MessageUpdatedEventArgs e)
		{
			CurrentStatus = e.Message;
		}

		public void Stop()
		{
			_ffmpeg?.Stop();
			_ffmpeg = null;
		}
	}
}
