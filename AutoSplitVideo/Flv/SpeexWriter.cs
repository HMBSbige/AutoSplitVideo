using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoSplitVideo.Flv
{
	internal class SpeexWriter : IAudioWriter
	{
		private const string _vendorString = "FLV Extract";
		private const uint _sampleRate = 16000;
		private const uint _msPerFrame = 20;
		private const uint _samplesPerFrame = _sampleRate / (1000 / _msPerFrame);
		private const int _targetPageDataSize = 4096;

		private FileStream _fs;
		private int _serialNumber;
		private List<OggPacket> _packetList;
		private int _packetListDataSize;
		private byte[] _pageBuff;
		private int _pageBuffOffset;
		private uint _pageSequenceNumber;
		private ulong _granulePosition;

		public SpeexWriter(string path, int serialNumber)
		{
			Path = path;
			_serialNumber = serialNumber;
			_fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 65536);
			_fs.Seek(28 + 80 + 28 + 8 + _vendorString.Length, SeekOrigin.Begin); // Speex header + Vorbis comment
			_packetList = new List<OggPacket>();
			_packetListDataSize = 0;
			_pageBuff = new byte[27 + 255 + _targetPageDataSize + 254]; // Header + max segment table + target data size + extra segment
			_pageBuffOffset = 0;
			_pageSequenceNumber = 2; // First audio packet
			_granulePosition = 0;
		}

		public void WriteChunk(byte[] chunk, uint timeStamp)
		{
			var subModeSizes = new[] { 0, 43, 119, 160, 220, 300, 364, 492, 79 };
			var wideBandSizes = new[] { 0, 36, 112, 192, 352 };
			var inBandSignalSizes = new[] { 1, 1, 4, 4, 4, 4, 4, 4, 8, 8, 16, 16, 32, 32, 64, 64 };
			var frameStart = -1;
			var frameEnd = 0;
			var offset = 0;
			var length = chunk.Length * 8;
			int x;

			while (length - offset >= 5)
			{
				x = BitHelper.Read(chunk, ref offset, 1);
				if (x != 0)
				{
					// wideband frame
					x = BitHelper.Read(chunk, ref offset, 3);
					if (x < 1 || x > 4) goto Error;
					offset += wideBandSizes[x] - 4;
				}
				else
				{
					x = BitHelper.Read(chunk, ref offset, 4);
					if (x >= 1 && x <= 8)
					{
						// narrowband frame
						if (frameStart != -1)
						{
							WriteFramePacket(chunk, frameStart, frameEnd);
						}
						frameStart = frameEnd;
						offset += subModeSizes[x] - 5;
					}
					else if (x == 15)
					{
						// terminator
						break;
					}
					else if (x == 14)
					{
						// in-band signal
						if (length - offset < 4) goto Error;
						x = BitHelper.Read(chunk, ref offset, 4);
						offset += inBandSignalSizes[x];
					}
					else if (x == 13)
					{
						// custom in-band signal
						if (length - offset < 5) goto Error;
						x = BitHelper.Read(chunk, ref offset, 5);
						offset += x * 8;
					}
					else goto Error;
				}
				frameEnd = offset;
			}
			if (offset > length) goto Error;

			if (frameStart != -1)
			{
				WriteFramePacket(chunk, frameStart, frameEnd);
			}

			return;

			Error:
			throw new Exception("Invalid Speex data.");
		}

		public void Finish()
		{
			WritePage();
			FlushPage(true);
			_fs.Seek(0, SeekOrigin.Begin);
			_pageSequenceNumber = 0;
			_granulePosition = 0;
			WriteSpeexHeaderPacket();
			WriteVorbisCommentPacket();
			FlushPage(false);
			_fs.Close();
		}

		public string Path { get; }

		private void WriteFramePacket(byte[] data, int startBit, int endBit)
		{
			var lengthBits = endBit - startBit;
			var frame = BitHelper.CopyBlock(data, startBit, lengthBits);
			if (lengthBits % 8 != 0)
			{
				frame[frame.Length - 1] |= (byte)(0xFF >> (lengthBits % 8 + 1)); // padding
			}
			AddPacket(frame, _samplesPerFrame, true);
		}

		private void WriteSpeexHeaderPacket()
		{
			var data = new byte[80];
			General.CopyBytes(data, 0, Encoding.ASCII.GetBytes("Speex   ")); // speex_string
			General.CopyBytes(data, 8, Encoding.ASCII.GetBytes("unknown")); // speex_version
			data[28] = 1; // speex_version_id
			data[32] = 80; // header_size
			General.CopyBytes(data, 36, BitConverterLE.GetBytes((uint)_sampleRate)); // rate
			data[40] = 1; // mode (e.g. narrowband, wideband)
			data[44] = 4; // mode_bitstream_version
			data[48] = 1; // nb_channels
			General.CopyBytes(data, 52, BitConverterLE.GetBytes(unchecked((uint)-1))); // bitrate
			General.CopyBytes(data, 56, BitConverterLE.GetBytes((uint)_samplesPerFrame)); // frame_size
			data[60] = 0; // vbr
			data[64] = 1; // frames_per_packet
			AddPacket(data, 0, false);
		}

		private void WriteVorbisCommentPacket()
		{
			var vendorStringBytes = Encoding.ASCII.GetBytes(_vendorString);
			var data = new byte[8 + vendorStringBytes.Length];
			data[0] = (byte)vendorStringBytes.Length;
			General.CopyBytes(data, 4, vendorStringBytes);
			AddPacket(data, 0, false);
		}

		private void AddPacket(byte[] data, uint sampleLength, bool delayWrite)
		{
			var packet = new OggPacket();
			if (data.Length >= 255)
			{
				throw new Exception("Packet exceeds maximum size.");
			}
			_granulePosition += sampleLength;
			packet.Data = data;
			packet.GranulePosition = _granulePosition;
			_packetList.Add(packet);
			_packetListDataSize += data.Length;
			if (!delayWrite || _packetListDataSize >= _targetPageDataSize || _packetList.Count == 255)
			{
				WritePage();
			}
		}

		private void WritePage()
		{
			if (_packetList.Count == 0) return;
			FlushPage(false);
			WriteToPage(BitConverterBE.GetBytes(0x4F676753U), 0, 4); // "OggS"
			WriteToPage((byte)0); // Stream structure version
			WriteToPage((byte)(_pageSequenceNumber == 0 ? 0x02 : 0)); // Page flags
			WriteToPage((ulong)_packetList[_packetList.Count - 1].GranulePosition); // Position in samples
			WriteToPage((uint)_serialNumber); // Stream serial number
			WriteToPage((uint)_pageSequenceNumber); // Page sequence number
			WriteToPage((uint)0); // Checksum
			WriteToPage((byte)_packetList.Count); // Page segment count
			foreach (var packet in _packetList)
			{
				WriteToPage((byte)packet.Data.Length);
			}
			foreach (var packet in _packetList)
			{
				WriteToPage(packet.Data, 0, packet.Data.Length);
			}
			_packetList.Clear();
			_packetListDataSize = 0;
			_pageSequenceNumber++;
		}

		private void FlushPage(bool isLastPage)
		{
			if (_pageBuffOffset == 0) return;
			if (isLastPage) _pageBuff[5] |= 0x04;
			var crc = OggCrc.Calculate(_pageBuff, 0, _pageBuffOffset);
			General.CopyBytes(_pageBuff, 22, BitConverterLE.GetBytes(crc));
			_fs.Write(_pageBuff, 0, _pageBuffOffset);
			_pageBuffOffset = 0;
		}

		private void WriteToPage(byte[] data, int offset, int length)
		{
			Buffer.BlockCopy(data, offset, _pageBuff, _pageBuffOffset, length);
			_pageBuffOffset += length;
		}

		private void WriteToPage(byte data)
		{
			WriteToPage(new[] { data }, 0, 1);
		}

		private void WriteToPage(uint data)
		{
			WriteToPage(BitConverterLE.GetBytes(data), 0, 4);
		}

		private void WriteToPage(ulong data)
		{
			WriteToPage(BitConverterLE.GetBytes(data), 0, 8);
		}

		private class OggPacket
		{
			public ulong GranulePosition;
			public byte[] Data;
		}
	}
}