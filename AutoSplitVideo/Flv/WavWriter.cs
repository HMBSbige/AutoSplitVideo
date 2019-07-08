using System;
using System.IO;

namespace AutoSplitVideo.Flv
{
	public class WavWriter
	{
		private BinaryWriter _bw;
		private bool _canSeek;
		private bool _wroteHeaders;
		private int _bitsPerSample;
		private int _channelCount;
		private int _sampleRate;
		private int _blockAlign;
		private long _finalSampleLen;

		public WavWriter(string path, int bitsPerSample, int channelCount, int sampleRate) :
			this(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read),
				bitsPerSample, channelCount, sampleRate)
		{
		}

		public WavWriter(Stream stream, int bitsPerSample, int channelCount, int sampleRate)
		{
			_bitsPerSample = bitsPerSample;
			_channelCount = channelCount;
			_sampleRate = sampleRate;
			_blockAlign = _channelCount * ((_bitsPerSample + 7) / 8);

			_bw = new BinaryWriter(stream);
			_canSeek = stream.CanSeek;
		}

		private void WriteHeaders()
		{
			const uint fccRIFF = 0x46464952;
			const uint fccWAVE = 0x45564157;
			const uint fccFormat = 0x20746D66;
			const uint fccData = 0x61746164;

			var dataChunkSize = GetDataChunkSize(_finalSampleLen);

			_bw.Write(fccRIFF);
			_bw.Write(dataChunkSize + (dataChunkSize & 1) + 36);
			_bw.Write(fccWAVE);

			_bw.Write(fccFormat);
			_bw.Write((uint)16);
			_bw.Write((ushort)1);
			_bw.Write((ushort)_channelCount);
			_bw.Write((uint)_sampleRate);
			_bw.Write((uint)(_sampleRate * _blockAlign));
			_bw.Write((ushort)_blockAlign);
			_bw.Write((ushort)_bitsPerSample);

			_bw.Write(fccData);
			_bw.Write(dataChunkSize);
		}

		private uint GetDataChunkSize(long sampleCount)
		{
			const long maxFileSize = 0x7FFFFFFEL;
			var dataSize = sampleCount * _blockAlign;
			if (dataSize + 44 > maxFileSize)
			{
				dataSize = (maxFileSize - 44) / _blockAlign * _blockAlign;
			}
			return (uint)dataSize;
		}

		public void Close()
		{
			if (((Position * _blockAlign) & 1) == 1)
			{
				_bw.Write((byte)0);
			}

			try
			{
				if (Position != _finalSampleLen)
				{
					if (_canSeek)
					{
						var dataChunkSize = GetDataChunkSize(Position);
						_bw.Seek(4, SeekOrigin.Begin);
						_bw.Write(dataChunkSize + (dataChunkSize & 1) + 36);
						_bw.Seek(40, SeekOrigin.Begin);
						_bw.Write(dataChunkSize);
					}
					else
					{
						throw new Exception("Samples written differs from the expected sample count.");
					}
				}
			}
			finally
			{
				_bw.Close();
				_bw = null;
			}
		}

		public long Position { get; private set; }

		public long FinalSampleCount
		{
			set => _finalSampleLen = value;
		}

		public void Write(byte[] buff, int sampleCount)
		{
			if (sampleCount <= 0) return;

			if (!_wroteHeaders)
			{
				WriteHeaders();
				_wroteHeaders = true;
			}

			_bw.Write(buff, 0, sampleCount * _blockAlign);
			Position += sampleCount;
		}
	}
}
