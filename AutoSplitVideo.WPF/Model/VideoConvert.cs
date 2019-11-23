using AutoSplitVideo.Utils;
using AutoSplitVideo.ViewModel;
using FlvProcessor;
using FlvProcessor.FlvExtract;
using System;
using System.IO;
using System.Threading.Tasks;

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
		private FlvFile _flvFile;

		public VideoConvert()
		{
			_currentStatus = @"未开始";
			_description = @"未知";
			_ffmpeg = null;
			_flvFile = null;
		}

		public void Convert(string inputPath, string outputPath, bool isDelete, bool deleteToRecycle, bool fixTimestamp)
		{
			Stop();
			if (fixTimestamp)
			{
				Task.Run(() =>
				{
					Description = $@"时间戳修复：""{inputPath}""";
					CurrentStatus = @"正在抽取";
					_flvFile = new FlvFile(inputPath);
					try
					{
						_flvFile.ExtractStreams(true, true, false, true);
					}
					catch (Exception e)
					{
						CurrentStatus = $@"[修复失败]{e.Message}";
						_flvFile.Dispose();
						return;
					}
					CurrentStatus = @"抽取完成";
					var video = _flvFile.VideoPath;
					var audio = _flvFile.AudioPath;
					_flvFile.Dispose();

					var parameters = $@"-i ""{video}"" -i ""{audio}"" -vcodec copy -acodec copy ""{outputPath}"" -y";
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

						File.Delete(video);
						File.Delete(audio);
					};
					Description = $@"封装：""{video}"" + ""{audio}"" => ""{outputPath}""";
					_ffmpeg.StartAsync(parameters);
				});
			}
			else
			{
				var parameters = $@"-i ""{inputPath}"" -c copy ""{outputPath}"" -y";
				_ffmpeg = new FFmpeg();
				_ffmpeg.MessageUpdated += FFmpeg_MessageUpdated;
				_ffmpeg.ProcessExited += (sender, args) =>
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
		}

		public void Split(string inputPath, string outputPath, string startTime, string duration)
		{
			var parameters = $@"-ss {startTime} -t {duration} -accurate_seek -i ""{inputPath}"" -codec copy -avoid_negative_ts 1 ""{outputPath}"" -y";
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
			_flvFile?.Dispose();
			_flvFile = null;
			_ffmpeg?.Stop();
			_ffmpeg = null;
		}
	}
}
