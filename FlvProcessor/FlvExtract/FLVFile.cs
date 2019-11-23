using FlvProcessor.FlvExtract.AudioWriter;
using FlvProcessor.FlvExtract.Interface;
using FlvProcessor.FlvExtract.Utils;
using FlvProcessor.FlvExtract.VideoWriter;
using System;
using System.Collections.Generic;
using System.IO;

namespace FlvProcessor.FlvExtract
{
	public class FlvFile : IDisposable
	{
		private static readonly string[] OutputExtensions = { ".avi", ".mp3", ".264", ".aac", ".spx", ".txt" };

		private readonly string _inputPath;
		private string _outputPathBase;
		private bool _overwrite;
		private FileStream _fs;
		private long _fileOffset;
		private readonly long _fileLength;
		private IAudioWriter _audioWriter;
		private IVideoWriter _videoWriter;
		private TimeCodeWriter _timeCodeWriter;
		private List<uint> _videoTimeStamps;
		private bool _extractAudio;
		private bool _extractVideo;
		private bool _extractTimeCodes;
		private readonly List<string> _warnings;

		public string VideoPath;
		public string AudioPath;

		public FlvFile(string path)
		{
			_inputPath = path;
			OutputDirectory = Path.GetDirectoryName(path);
			_warnings = new List<string>();
			_fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
			_fileOffset = 0;
			_fileLength = _fs.Length;
		}

		public string OutputDirectory { get; set; }

		public FractionUInt32? AverageFrameRate { get; private set; }

		public FractionUInt32? TrueFrameRate { get; private set; }

		public string[] Warnings => _warnings.ToArray();

		public bool ExtractedAudio { get; private set; }

		public bool ExtractedVideo { get; private set; }

		public bool ExtractedTimeCodes { get; private set; }

		public void ExtractStreams(bool extractAudio, bool extractVideo, bool extractTimeCodes, bool overwrite)
		{
			_outputPathBase = Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(_inputPath) ?? throw new InvalidOperationException());
			_overwrite = overwrite;
			_extractAudio = extractAudio;
			_extractVideo = extractVideo;
			_extractTimeCodes = extractTimeCodes;
			_videoTimeStamps = new List<uint>();

			Seek(0);
			if (_fileLength < 4 || ReadUInt32() != 0x464C5601)
			{
				if (_fileLength >= 8 && ReadUInt32() == 0x66747970)
				{
					throw new Exception("This is a MP4 file. YAMB or MP4Box can be used to extract streams.");
				}

				throw new Exception("This isn't a FLV file.");
			}

			if (Array.IndexOf(OutputExtensions, Path.GetExtension(_inputPath)?.ToLowerInvariant()) != -1)
			{
				// Can't have the same extension as files we output
				throw new Exception("Please change the extension of this FLV file.");
			}

			if (!Directory.Exists(OutputDirectory))
			{
				throw new Exception("Output directory doesn't exist.");
			}

			ReadUInt8(); //flags
			var dataOffset = ReadUInt32();

			Seek(dataOffset);

			ReadUInt32(); //prevTagSize
			while (_fileOffset < _fileLength)
			{
				if (!ReadTag()) break;
				if (_fileLength - _fileOffset < 4) break;
				ReadUInt32(); //prevTagSize
			}

			AverageFrameRate = CalculateAverageFrameRate();
			TrueFrameRate = CalculateTrueFrameRate();

			CloseOutput(AverageFrameRate, false);
		}

		private void CloseOutput(FractionUInt32? averageFrameRate, bool disposing)
		{
			if (_videoWriter != null)
			{
				_videoWriter.Finish(averageFrameRate ?? new FractionUInt32(25, 1));
				if (disposing && _videoWriter.Path != null)
				{
					try
					{
						File.Delete(_videoWriter.Path);
					}
					catch
					{
						// ignored
					}
				}
				_videoWriter = null;
			}
			if (_audioWriter != null)
			{
				_audioWriter.Finish();
				if (disposing && _audioWriter.Path != null)
				{
					try
					{
						File.Delete(_audioWriter.Path);
					}
					catch
					{
						// ignored
					}
				}
				_audioWriter = null;
			}
			if (_timeCodeWriter != null)
			{
				_timeCodeWriter.Finish();
				if (disposing && _timeCodeWriter.Path != null)
				{
					try
					{
						File.Delete(_timeCodeWriter.Path);
					}
					catch
					{
						// ignored
					}
				}
				_timeCodeWriter = null;
			}
		}

		private bool ReadTag()
		{
			if (_fileLength - _fileOffset < 11)
			{
				return false;
			}

			// Read tag header
			var tagType = ReadUInt8();
			var dataSize = ReadUInt24();
			var timeStamp = ReadUInt24();
			timeStamp |= ReadUInt8() << 24;
			ReadUInt24(); //streamID

			// Read tag data
			if (dataSize == 0)
			{
				return true;
			}
			if (_fileLength - _fileOffset < dataSize)
			{
				return false;
			}
			var mediaInfo = ReadUInt8();
			dataSize -= 1;
			var data = ReadBytes((int)dataSize);

			if (tagType == 0x8)
			{
				// Audio
				if (_audioWriter == null)
				{
					_audioWriter = _extractAudio ? GetAudioWriter(mediaInfo) : new DummyAudioWriter();
					ExtractedAudio = !(_audioWriter is DummyAudioWriter);
				}
				_audioWriter.WriteChunk(data, timeStamp);
			}
			else if (tagType == 0x9 && mediaInfo >> 4 != 5)
			{
				// Video
				if (_videoWriter == null)
				{
					_videoWriter = _extractVideo ? GetVideoWriter(mediaInfo) : new DummyVideoWriter();
					ExtractedVideo = !(_videoWriter is DummyVideoWriter);
				}
				if (_timeCodeWriter == null)
				{
					var path = $@"{_outputPathBase}.txt";
					_timeCodeWriter = new TimeCodeWriter(_extractTimeCodes && CanWriteTo(path) ? path : null);
					ExtractedTimeCodes = _extractTimeCodes;
				}
				_videoTimeStamps.Add(timeStamp);
				_videoWriter.WriteChunk(data, timeStamp, (int)((mediaInfo & 0xF0) >> 4));
				_timeCodeWriter.Write(timeStamp);
			}

			return true;
		}

		private IAudioWriter GetAudioWriter(uint mediaInfo)
		{
			var format = mediaInfo >> 4;
			var rate = (mediaInfo >> 2) & 0x3;
			var bits = (mediaInfo >> 1) & 0x1;
			var chans = mediaInfo & 0x1;

			string typeStr;

			switch (format)
			{
				case 2:
				case 14:
				{
					// MP3
					AudioPath = $@"{_outputPathBase}.mp3";
					if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
					return new MP3Writer(AudioPath, _warnings);
				}
				case 0:
				case 3:
				{
					// PCM
					var sampleRate = rate switch
					{
						0 => 5512,
						1 => 11025,
						2 => 22050,
						3 => 44100,
						_ => 0
					};
					AudioPath = $@"{_outputPathBase}.wav";
					if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
					if (format == 0)
					{
						_warnings.Add("PCM byte order unspecified, assuming little endian.");
					}
					return new MyWavWriter(AudioPath, bits == 1 ? 16 : 8, chans == 1 ? 2 : 1, sampleRate);
				}
				case 10:
				{
					// AAC
					AudioPath = $@"{_outputPathBase}.aac";
					if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
					return new AACWriter(AudioPath);
				}
				case 11:
				{
					// Speex
					AudioPath = $@"{_outputPathBase}.spx";
					if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
					return new SpeexWriter(AudioPath, (int)(_fileLength & 0xFFFFFFFF));
				}
				case 1:
					typeStr = "ADPCM";
					break;
				case 4:
				case 5:
				case 6:
					typeStr = "Nellymoser";
					break;
				default:
					typeStr = $@"format={format}";
					break;
			}

			_warnings.Add($@"Unable to extract audio ({typeStr} is unsupported).");

			return new DummyAudioWriter();
		}

		private IVideoWriter GetVideoWriter(uint mediaInfo)
		{
			var codecId = mediaInfo & 0x0F;

			string typeStr;
			switch (codecId)
			{
				case 2:
				case 4:
				case 5:
				{
					VideoPath = $@"{_outputPathBase}.avi";
					if (!CanWriteTo(VideoPath)) return new DummyVideoWriter();
					return new AVIWriter(VideoPath, (int)codecId, _warnings);
				}
				case 7:
				{
					VideoPath = $@"{_outputPathBase}.264";
					if (!CanWriteTo(VideoPath)) return new DummyVideoWriter();
					return new RawH264Writer(VideoPath);
				}
				case 3:
				{
					typeStr = @"Screen";
					break;
				}
				case 6:
				{
					typeStr = @"Screen2";
					break;
				}
				default:
				{
					typeStr = $@"codecID={codecId}";
					break;
				}
			}

			_warnings.Add($@"Unable to extract video ({typeStr} is unsupported).");

			return new DummyVideoWriter();
		}

		private bool CanWriteTo(string path)
		{
			return !File.Exists(path) || _overwrite;
		}

		private FractionUInt32? CalculateAverageFrameRate()
		{
			var frameCount = _videoTimeStamps.Count;

			if (frameCount > 1)
			{
				FractionUInt32 frameRate;
				frameRate.N = (uint)(frameCount - 1) * 1000;
				frameRate.D = _videoTimeStamps[frameCount - 1] - _videoTimeStamps[0];
				frameRate.Reduce();
				return frameRate;
			}
			return null;
		}

		private FractionUInt32? CalculateTrueFrameRate()
		{
			var deltaCount = new Dictionary<uint, uint>();
			uint delta, count;

			// Calculate the distance between the timestamps, count how many times each delta appears
			for (var i = 1; i < _videoTimeStamps.Count; i++)
			{
				var deltaS = (int)(_videoTimeStamps[i] - (long)_videoTimeStamps[i - 1]);

				if (deltaS <= 0) continue;
				delta = (uint)deltaS;

				if (deltaCount.ContainsKey(delta))
				{
					deltaCount[delta] += 1;
				}
				else
				{
					deltaCount.Add(delta, 1);
				}
			}

			var threshold = _videoTimeStamps.Count / 10;
			var minDelta = uint.MaxValue;

			// Find the smallest delta that made up at least 10% of the frames (grouping in delta+1
			// because of rounding, e.g. a NTSC video will have deltas of 33 and 34 ms)
			foreach (var deltaItem in deltaCount)
			{
				delta = deltaItem.Key;
				count = deltaItem.Value;

				if (deltaCount.ContainsKey(delta + 1))
				{
					count += deltaCount[delta + 1];
				}

				if (count >= threshold && delta < minDelta)
				{
					minDelta = delta;
				}
			}

			// Calculate the frame rate based on the smallest delta, and delta+1 if present
			if (minDelta != uint.MaxValue)
			{
				count = deltaCount[minDelta];
				var totalTime = minDelta * count;
				var totalFrames = count;

				if (deltaCount.ContainsKey(minDelta + 1))
				{
					count = deltaCount[minDelta + 1];
					totalTime += (minDelta + 1) * count;
					totalFrames += count;
				}

				if (totalTime != 0)
				{
					FractionUInt32 frameRate;
					frameRate.N = totalFrames * 1000;
					frameRate.D = totalTime;
					frameRate.Reduce();
					return frameRate;
				}
			}

			// Unable to calculate frame rate
			return null;
		}

		private void Seek(long offset)
		{
			_fs.Seek(offset, SeekOrigin.Begin);
			_fileOffset = offset;
		}

		private uint ReadUInt8()
		{
			_fileOffset += 1;
			return (uint)_fs.ReadByte();
		}

		private uint ReadUInt24()
		{
			var x = new byte[4];
			_fs.Read(x, 1, 3);
			_fileOffset += 3;
			return BitConverterBE.ToUInt32(x, 0);
		}

		private uint ReadUInt32()
		{
			var x = new byte[4];
			_fs.Read(x, 0, 4);
			_fileOffset += 4;
			return BitConverterBE.ToUInt32(x, 0);
		}

		private byte[] ReadBytes(int length)
		{
			var buff = new byte[length];
			_fs.Read(buff, 0, length);
			_fileOffset += length;
			return buff;
		}

		#region IDisposable Support
		private bool _disposedValue;

		// instance based lock
		private readonly object _lock = new object();

		protected virtual void Dispose(bool disposing)
		{
			lock (_lock)
			{
				if (_disposedValue)
				{
					return;
				}
				_disposedValue = true;
			}

			if (disposing)
			{
				if (_fs != null)
				{
					_fs.Close();
					_fs = null;
				}
				CloseOutput(null, true);
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
