using FlvProcessor.FlvExtract.Interface;
using FlvProcessor.FlvExtract.Utils;
using System.Collections.Generic;
using System.IO;

namespace FlvProcessor.FlvExtract.AudioWriter
{
	internal class MP3Writer : IAudioWriter
	{
		private FileStream _fs;
		private List<string> _warnings;
		private List<byte[]> _chunkBuffer;
		private List<uint> _frameOffsets;
		private uint _totalFrameLength;
		private bool _isVBR;
		private bool _delayWrite;
		private bool _hasVBRHeader;
		private bool _writeVBRHeader;
		private int _firstBitRate;
		private int _mpegVersion;
		private int _sampleRate;
		private int _channelMode;
		private uint _firstFrameHeader;

		public MP3Writer(string path, List<string> warnings)
		{
			Path = path;
			_fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 65536);
			_warnings = warnings;
			_chunkBuffer = new List<byte[]>();
			_frameOffsets = new List<uint>();
			_delayWrite = true;
		}

		public void WriteChunk(byte[] chunk, uint timeStamp)
		{
			_chunkBuffer.Add(chunk);
			ParseMP3Frames(chunk);
			if (_delayWrite && _totalFrameLength >= 65536)
			{
				_delayWrite = false;
			}
			if (!_delayWrite)
			{
				Flush();
			}
		}

		public void Finish()
		{
			Flush();
			if (_writeVBRHeader)
			{
				_fs.Seek(0, SeekOrigin.Begin);
				WriteVBRHeader(false);
			}
			_fs.Close();
		}

		public string Path { get; }

		private void Flush()
		{
			foreach (var chunk in _chunkBuffer)
			{
				_fs.Write(chunk, 0, chunk.Length);
			}
			_chunkBuffer.Clear();
		}

		private void ParseMP3Frames(byte[] buff)
		{
			var MPEG1BitRate = new[] { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 };
			var MPEG2XBitRate = new[] { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0 };
			var MPEG1SampleRate = new[] { 44100, 48000, 32000, 0 };
			var MPEG20SampleRate = new[] { 22050, 24000, 16000, 0 };
			var MPEG25SampleRate = new[] { 11025, 12000, 8000, 0 };

			var offset = 0;
			var length = buff.Length;

			while (length >= 4)
			{
				var header = (ulong)BitConverterBE.ToUInt32(buff, offset) << 32;
				if (BitHelper.Read(ref header, 11) != 0x7FF)
				{
					break;
				}
				var mpegVersion = BitHelper.Read(ref header, 2);
				var layer = BitHelper.Read(ref header, 2);
				BitHelper.Read(ref header, 1);
				var bitRate = BitHelper.Read(ref header, 4);
				var sampleRate = BitHelper.Read(ref header, 2);
				var padding = BitHelper.Read(ref header, 1);
				BitHelper.Read(ref header, 1);
				var channelMode = BitHelper.Read(ref header, 2);

				if (mpegVersion == 1 || layer != 1 || bitRate == 0 || bitRate == 15 || sampleRate == 3)
				{
					break;
				}

				bitRate = (mpegVersion == 3 ? MPEG1BitRate[bitRate] : MPEG2XBitRate[bitRate]) * 1000;

				if (mpegVersion == 3)
					sampleRate = MPEG1SampleRate[sampleRate];
				else if (mpegVersion == 2)
					sampleRate = MPEG20SampleRate[sampleRate];
				else
					sampleRate = MPEG25SampleRate[sampleRate];

				var frameLen = GetFrameLength(mpegVersion, bitRate, sampleRate, padding);
				if (frameLen > length)
				{
					break;
				}

				var isVBRHeaderFrame = false;
				if (_frameOffsets.Count == 0)
				{
					// Check for an existing VBR header just to be safe (I haven't seen any in FLVs)
					var o = offset + GetFrameDataOffset(mpegVersion, channelMode);
					if (BitConverterBE.ToUInt32(buff, o) == 0x58696E67)
					{ // "Xing"
						isVBRHeaderFrame = true;
						_delayWrite = false;
						_hasVBRHeader = true;
					}
				}

				if (isVBRHeaderFrame) { }
				else if (_firstBitRate == 0)
				{
					_firstBitRate = bitRate;
					_mpegVersion = mpegVersion;
					_sampleRate = sampleRate;
					_channelMode = channelMode;
					_firstFrameHeader = BitConverterBE.ToUInt32(buff, offset);
				}
				else if (!_isVBR && bitRate != _firstBitRate)
				{
					_isVBR = true;
					if (_hasVBRHeader) { }
					else if (_delayWrite)
					{
						WriteVBRHeader(true);
						_writeVBRHeader = true;
						_delayWrite = false;
					}
					else
					{
						_warnings.Add("Detected VBR too late, cannot add VBR header.");
					}
				}

				_frameOffsets.Add(_totalFrameLength + (uint)offset);

				offset += frameLen;
				length -= frameLen;
			}

			_totalFrameLength += (uint)buff.Length;
		}

		private void WriteVBRHeader(bool isPlaceholder)
		{
			var buff = new byte[GetFrameLength(_mpegVersion, 64000, _sampleRate, 0)];
			if (!isPlaceholder)
			{
				var header = _firstFrameHeader;
				var dataOffset = GetFrameDataOffset(_mpegVersion, _channelMode);
				header &= 0xFFFF0DFF; // Clear bit rate and padding fields
				header |= 0x00010000; // Set protection bit (indicates that CRC is NOT present)
				header |= (uint)(_mpegVersion == 3 ? 5 : 8) << 12; // 64 kbit/sec
				General.CopyBytes(buff, 0, BitConverterBE.GetBytes(header));
				General.CopyBytes(buff, dataOffset, BitConverterBE.GetBytes(0x58696E67)); // "Xing"
				General.CopyBytes(buff, dataOffset + 4, BitConverterBE.GetBytes((uint)0x7)); // Flags
				General.CopyBytes(buff, dataOffset + 8, BitConverterBE.GetBytes((uint)_frameOffsets.Count)); // Frame count
				General.CopyBytes(buff, dataOffset + 12, BitConverterBE.GetBytes(_totalFrameLength)); // File length
				for (var i = 0; i < 100; i++)
				{
					var frameIndex = (int)(i / 100.0 * _frameOffsets.Count);
					buff[dataOffset + 16 + i] = (byte)(_frameOffsets[frameIndex] / (double)_totalFrameLength * 256.0);
				}
			}
			_fs.Write(buff, 0, buff.Length);
		}

		private static int GetFrameLength(int mpegVersion, int bitRate, int sampleRate, int padding)
		{
			return (mpegVersion == 3 ? 144 : 72) * bitRate / sampleRate + padding;
		}

		private static int GetFrameDataOffset(int mpegVersion, int channelMode)
		{
			return 4 + (mpegVersion == 3 ?
						   channelMode == 3 ? 17 : 32 :
						   channelMode == 3 ? 9 : 17);
		}
	}
}