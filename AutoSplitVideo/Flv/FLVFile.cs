using System;
using System.Collections.Generic;
using System.IO;

namespace AutoSplitVideo.Flv
{
	public class FlvFile : IDisposable
	{
		private static readonly string[] _outputExtensions = { ".avi", ".mp3", ".264", ".aac", ".spx", ".txt" };

		private string _inputPath;
		private string _outputPathBase;
		private bool _overwrite;
		private FileStream _fs;
		private long _fileOffset, _fileLength;
		private IAudioWriter _audioWriter;
		private IVideoWriter _videoWriter;
		private TimeCodeWriter _timeCodeWriter;
		private List<uint> _videoTimeStamps;
		private bool _extractAudio;
		private bool _extractVideo;
		private bool _extractTimeCodes;
		private List<string> _warnings;

		public string VideoPath;
		public string AudioPath = null;

		public FlvFile(string path)
		{
			_inputPath = path;
			OutputDirectory = Path.GetDirectoryName(path);
			_warnings = new List<string>();
			_fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536);
			_fileOffset = 0;
			_fileLength = _fs.Length;
		}

		public void Dispose()
		{
			if (_fs != null)
			{
				_fs.Close();
				_fs = null;
			}
			CloseOutput(null, true);
		}

		public void Close()
		{
			Dispose();
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
			uint dataOffset, flags, prevTagSize;

			_outputPathBase = Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(_inputPath));
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
				else
				{
					throw new Exception("This isn't a FLV file.");
				}
			}

			if (Array.IndexOf(_outputExtensions, Path.GetExtension(_inputPath).ToLowerInvariant()) != -1)
			{
				// Can't have the same extension as files we output
				throw new Exception("Please change the extension of this FLV file.");
			}

			if (!Directory.Exists(OutputDirectory))
			{
				throw new Exception("Output directory doesn't exist.");
			}

			flags = ReadUInt8();
			dataOffset = ReadUInt32();

			Seek(dataOffset);

			prevTagSize = ReadUInt32();
			while (_fileOffset < _fileLength)
			{
				if (!ReadTag()) break;
				if (_fileLength - _fileOffset < 4) break;
				prevTagSize = ReadUInt32();
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
					try { File.Delete(_videoWriter.Path); }
					catch { }
				}
				_videoWriter = null;
			}
			if (_audioWriter != null)
			{
				_audioWriter.Finish();
				if (disposing && _audioWriter.Path != null)
				{
					try { File.Delete(_audioWriter.Path); }
					catch { }
				}
				_audioWriter = null;
			}
			if (_timeCodeWriter != null)
			{
				_timeCodeWriter.Finish();
				if (disposing && _timeCodeWriter.Path != null)
				{
					try { File.Delete(_timeCodeWriter.Path); }
					catch { }
				}
				_timeCodeWriter = null;
			}
		}

		private bool ReadTag()
		{
			uint tagType, dataSize, timeStamp, streamID, mediaInfo;
			byte[] data;

			if (_fileLength - _fileOffset < 11)
			{
				return false;
			}

			// Read tag header
			tagType = ReadUInt8();
			dataSize = ReadUInt24();
			timeStamp = ReadUInt24();
			timeStamp |= ReadUInt8() << 24;
			streamID = ReadUInt24();

			// Read tag data
			if (dataSize == 0)
			{
				return true;
			}
			if (_fileLength - _fileOffset < dataSize)
			{
				return false;
			}
			mediaInfo = ReadUInt8();
			dataSize -= 1;
			data = ReadBytes((int)dataSize);

			if (tagType == 0x8)
			{  // Audio
				if (_audioWriter == null)
				{
					_audioWriter = _extractAudio ? GetAudioWriter(mediaInfo) : new DummyAudioWriter();
					ExtractedAudio = !(_audioWriter is DummyAudioWriter);
				}
				_audioWriter.WriteChunk(data, timeStamp);
			}
			else if (tagType == 0x9 && mediaInfo >> 4 != 5)
			{ // Video
				if (_videoWriter == null)
				{
					_videoWriter = _extractVideo ? GetVideoWriter(mediaInfo) : new DummyVideoWriter();
					ExtractedVideo = !(_videoWriter is DummyVideoWriter);
				}
				if (_timeCodeWriter == null)
				{
					var path = _outputPathBase + ".txt";
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

			if (format == 2 || format == 14)
			{ // MP3
				AudioPath = _outputPathBase + ".mp3";
				if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
				return new MP3Writer(AudioPath, _warnings);
			}
			else if (format == 0 || format == 3)
			{ // PCM
				var sampleRate = 0;
				switch (rate)
				{
					case 0: sampleRate = 5512; break;
					case 1: sampleRate = 11025; break;
					case 2: sampleRate = 22050; break;
					case 3: sampleRate = 44100; break;
				}
				AudioPath = _outputPathBase + ".wav";
				if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
				if (format == 0)
				{
					_warnings.Add("PCM byte order unspecified, assuming little endian.");
				}
				return new MyWavWriter(AudioPath, bits == 1 ? 16 : 8,
					chans == 1 ? 2 : 1, sampleRate);
			}
			else if (format == 10)
			{ // AAC
				AudioPath = _outputPathBase + ".aac";
				if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
				return new AACWriter(AudioPath);
			}
			else if (format == 11)
			{ // Speex
				AudioPath = _outputPathBase + ".spx";
				if (!CanWriteTo(AudioPath)) return new DummyAudioWriter();
				return new SpeexWriter(AudioPath, (int)(_fileLength & 0xFFFFFFFF));
			}
			else
			{
				string typeStr;

				if (format == 1)
					typeStr = "ADPCM";
				else if (format == 4 || format == 5 || format == 6)
					typeStr = "Nellymoser";
				else
					typeStr = "format=" + format.ToString();

				_warnings.Add("Unable to extract audio (" + typeStr + " is unsupported).");

				return new DummyAudioWriter();
			}
		}

		private IVideoWriter GetVideoWriter(uint mediaInfo)
		{
			var codecID = mediaInfo & 0x0F;

			if (codecID == 2 || codecID == 4 || codecID == 5)
			{
				VideoPath = $@"{_outputPathBase}.avi";
				if (!CanWriteTo(VideoPath)) return new DummyVideoWriter();
				return new AVIWriter(VideoPath, (int)codecID, _warnings);
			}
			else if (codecID == 7)
			{
				VideoPath = $@"{_outputPathBase}.264";
				if (!CanWriteTo(VideoPath)) return new DummyVideoWriter();
				return new RawH264Writer(VideoPath);
			}
			else
			{
				string typeStr;

				if (codecID == 3)
					typeStr = "Screen";
				else if (codecID == 6)
					typeStr = "Screen2";
				else
					typeStr = "codecID=" + codecID;

				_warnings.Add("Unable to extract video (" + typeStr + " is unsupported).");

				return new DummyVideoWriter();
			}
		}

		private bool CanWriteTo(string path)
		{
			if (File.Exists(path))
			{
				return _overwrite;
			}
			return true;
		}

		private FractionUInt32? CalculateAverageFrameRate()
		{
			FractionUInt32 frameRate;
			var frameCount = _videoTimeStamps.Count;

			if (frameCount > 1)
			{
				frameRate.N = (uint)(frameCount - 1) * 1000;
				frameRate.D = _videoTimeStamps[frameCount - 1] - _videoTimeStamps[0];
				frameRate.Reduce();
				return frameRate;
			}
			else
			{
				return null;
			}
		}

		private FractionUInt32? CalculateTrueFrameRate()
		{
			FractionUInt32 frameRate;
			var deltaCount = new Dictionary<uint, uint>();
			int i, threshold;
			uint delta, count, minDelta;

			// Calculate the distance between the timestamps, count how many times each delta appears
			for (i = 1; i < _videoTimeStamps.Count; i++)
			{
				var deltaS = (int)((long)_videoTimeStamps[i] - (long)_videoTimeStamps[i - 1]);

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

			threshold = _videoTimeStamps.Count / 10;
			minDelta = UInt32.MaxValue;

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
			if (minDelta != UInt32.MaxValue)
			{
				uint totalTime, totalFrames;

				count = deltaCount[minDelta];
				totalTime = minDelta * count;
				totalFrames = count;

				if (deltaCount.ContainsKey(minDelta + 1))
				{
					count = deltaCount[minDelta + 1];
					totalTime += (minDelta + 1) * count;
					totalFrames += count;
				}

				if (totalTime != 0)
				{
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
	}
}
