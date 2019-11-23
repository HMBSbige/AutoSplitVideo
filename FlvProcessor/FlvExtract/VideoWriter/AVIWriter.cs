using FlvProcessor.FlvExtract.Interface;
using FlvProcessor.FlvExtract.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlvProcessor.FlvExtract.VideoWriter
{
	internal class AVIWriter : IVideoWriter
	{
		private BinaryWriter _bw;
		private int _codecID;
		private int _width;
		private int _height;
		private int _frameCount;
		private uint _moviDataSize;
		private uint _indexChunkSize;
		private List<uint> _index;
		private bool _isAlphaWriter;
		private AVIWriter _alphaWriter;
		private List<string> _warnings;

		// Chunk:          Off:  Len:
		//
		// RIFF AVI          0    12
		//   LIST hdrl      12    12
		//     avih         24    64
		//     LIST strl    88    12
		//       strh      100    64
		//       strf      164    48
		//   LIST movi     212    12
		//     (frames)    224   ???
		//   idx1          ???   ???

		public AVIWriter(string path, int codecID, List<string> warnings) :
				this(path, codecID, warnings, false)
		{ }

		private AVIWriter(string path, int codecID, List<string> warnings, bool isAlphaWriter)
		{
			if (codecID != 2 && codecID != 4 && codecID != 5)
			{
				throw new Exception("Unsupported video codec.");
			}

			var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);

			Path = path;
			_bw = new BinaryWriter(fs);
			_codecID = codecID;
			_warnings = warnings;
			_isAlphaWriter = isAlphaWriter;

			if (codecID == 5 && !_isAlphaWriter)
			{
				_alphaWriter = new AVIWriter(path.Substring(0, path.Length - 4) + ".alpha.avi", codecID, warnings, true);
			}

			WriteFourCC("RIFF");
			_bw.Write((uint)0); // chunk size
			WriteFourCC("AVI ");

			WriteFourCC("LIST");
			_bw.Write((uint)192);
			WriteFourCC("hdrl");

			WriteFourCC("avih");
			_bw.Write((uint)56);
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0x10);
			_bw.Write((uint)0); // frame count
			_bw.Write((uint)0);
			_bw.Write((uint)1);
			_bw.Write((uint)0);
			_bw.Write((uint)0); // width
			_bw.Write((uint)0); // height
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0);

			WriteFourCC("LIST");
			_bw.Write((uint)116);
			WriteFourCC("strl");

			WriteFourCC("strh");
			_bw.Write((uint)56);
			WriteFourCC("vids");
			WriteFourCC(CodecFourCC);
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0); // frame rate denominator
			_bw.Write((uint)0); // frame rate numerator
			_bw.Write((uint)0);
			_bw.Write((uint)0); // frame count
			_bw.Write((uint)0);
			_bw.Write(-1);
			_bw.Write((uint)0);
			_bw.Write((ushort)0);
			_bw.Write((ushort)0);
			_bw.Write((ushort)0); // width
			_bw.Write((ushort)0); // height

			WriteFourCC("strf");
			_bw.Write((uint)40);
			_bw.Write((uint)40);
			_bw.Write((uint)0); // width
			_bw.Write((uint)0); // height
			_bw.Write((ushort)1);
			_bw.Write((ushort)24);
			WriteFourCC(CodecFourCC);
			_bw.Write((uint)0); // biSizeImage
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0);
			_bw.Write((uint)0);

			WriteFourCC("LIST");
			_bw.Write((uint)0); // chunk size
			WriteFourCC("movi");

			_index = new List<uint>();
		}

		public void WriteChunk(byte[] chunk, uint timeStamp, int frameType)
		{
			var offset = 0;
			var len = chunk.Length;
			if (_codecID == 4)
			{
				offset = 1;
				len -= 1;
			}
			if (_codecID == 5)
			{
				offset = 4;
				if (len >= 4)
				{
					var alphaOffset = (int)BitConverterBE.ToUInt32(chunk, 0) & 0xFFFFFF;
					if (!_isAlphaWriter)
					{
						len = alphaOffset;
					}
					else
					{
						offset += alphaOffset;
						len -= offset;
					}
				}
				else
				{
					len = 0;
				}
			}
			len = Math.Max(len, 0);
			len = Math.Min(len, chunk.Length - offset);

			_index.Add(frameType == 1 ? 0x10 : (uint)0);
			_index.Add(_moviDataSize + 4);
			_index.Add((uint)len);

			if (_width == 0 && _height == 0)
			{
				GetFrameSize(chunk);
			}

			WriteFourCC("00dc");
			_bw.Write(len);
			_bw.Write(chunk, offset, len);

			if (len % 2 != 0)
			{
				_bw.Write((byte)0);
				++len;
			}
			_moviDataSize += (uint)len + 8;
			++_frameCount;

			_alphaWriter?.WriteChunk(chunk, timeStamp, frameType);
		}

		private void GetFrameSize(byte[] chunk)
		{
			if (_codecID == 2)
			{
				// Reference: flv_h263_decode_picture_header from libavcodec's h263.c

				if (chunk.Length < 10) return;

				if (chunk[0] != 0 || chunk[1] != 0)
				{
					return;
				}

				var x = BitConverterBE.ToUInt64(chunk, 2);

				if (BitHelper.Read(ref x, 1) != 1)
				{
					return;
				}
				BitHelper.Read(ref x, 5);
				BitHelper.Read(ref x, 8);

				var format = BitHelper.Read(ref x, 3);
				switch (format)
				{
					case 0:
						_width = BitHelper.Read(ref x, 8);
						_height = BitHelper.Read(ref x, 8);
						break;
					case 1:
						_width = BitHelper.Read(ref x, 16);
						_height = BitHelper.Read(ref x, 16);
						break;
					case 2:
						_width = 352;
						_height = 288;
						break;
					case 3:
						_width = 176;
						_height = 144;
						break;
					case 4:
						_width = 128;
						_height = 96;
						break;
					case 5:
						_width = 320;
						_height = 240;
						break;
					case 6:
						_width = 160;
						_height = 120;
						break;
					default:
						return;
				}
			}
			else if (_codecID == 4 || _codecID == 5)
			{
				// Reference: vp6_parse_header from libavcodec's vp6.c

				var skip = _codecID == 4 ? 1 : 4;
				if (chunk.Length < skip + 8) return;
				var x = BitConverterBE.ToUInt64(chunk, skip);

				var deltaFrameFlag = BitHelper.Read(ref x, 1);
				BitHelper.Read(ref x, 6); //quant
				var separatedCoeffFlag = BitHelper.Read(ref x, 1);
				BitHelper.Read(ref x, 5); //subVersion
				var filterHeader = BitHelper.Read(ref x, 2);
				BitHelper.Read(ref x, 1); //interlacedFlag

				if (deltaFrameFlag != 0)
				{
					return;
				}
				if (separatedCoeffFlag != 0 || filterHeader == 0)
				{
					BitHelper.Read(ref x, 16);
				}

				_height = BitHelper.Read(ref x, 8) * 16;
				_width = BitHelper.Read(ref x, 8) * 16;

				// chunk[0] contains the width and height (4 bits each, respectively) that should
				// be cropped off during playback, which will be non-zero if the encoder padded
				// the frames to a macroblock boundary.  But if you use this adjusted size in the
				// AVI header, DirectShow seems to ignore it, and it can cause stride or chroma
				// alignment problems with VFW if the width/height aren't multiples of 4.
				if (!_isAlphaWriter)
				{
					var cropX = chunk[0] >> 4;
					var cropY = chunk[0] & 0x0F;
					if ((cropX != 0 || cropY != 0) && !_isAlphaWriter)
					{
						_warnings.Add($@"Suggested cropping: {cropX} pixels from right, {cropY} pixels from bottom.");
					}
				}
			}
		}

		private string CodecFourCC
		{
			get
			{
				if (_codecID == 2)
				{
					return "FLV1";
				}
				if (_codecID == 4 || _codecID == 5)
				{
					return "VP6F";
				}
				return "NULL";
			}
		}

		private void WriteIndexChunk()
		{
			var indexDataSize = (uint)_frameCount * 16;

			WriteFourCC("idx1");
			_bw.Write(indexDataSize);

			for (var i = 0; i < _frameCount; i++)
			{
				WriteFourCC("00dc");
				_bw.Write(_index[i * 3 + 0]);
				_bw.Write(_index[i * 3 + 1]);
				_bw.Write(_index[i * 3 + 2]);
			}

			_indexChunkSize = indexDataSize + 8;
		}

		public void Finish(FractionUInt32 averageFrameRate)
		{
			WriteIndexChunk();

			_bw.BaseStream.Seek(4, SeekOrigin.Begin);
			_bw.Write(224 + _moviDataSize + _indexChunkSize - 8);

			_bw.BaseStream.Seek(24 + 8, SeekOrigin.Begin);
			_bw.Write((uint)0);
			_bw.BaseStream.Seek(12, SeekOrigin.Current);
			_bw.Write((uint)_frameCount);
			_bw.BaseStream.Seek(12, SeekOrigin.Current);
			_bw.Write((uint)_width);
			_bw.Write((uint)_height);

			_bw.BaseStream.Seek(100 + 28, SeekOrigin.Begin);
			_bw.Write(averageFrameRate.D);
			_bw.Write(averageFrameRate.N);
			_bw.BaseStream.Seek(4, SeekOrigin.Current);
			_bw.Write((uint)_frameCount);
			_bw.BaseStream.Seek(16, SeekOrigin.Current);
			_bw.Write((ushort)_width);
			_bw.Write((ushort)_height);

			_bw.BaseStream.Seek(164 + 12, SeekOrigin.Begin);
			_bw.Write((uint)_width);
			_bw.Write((uint)_height);
			_bw.BaseStream.Seek(8, SeekOrigin.Current);
			_bw.Write((uint)(_width * _height * 6));

			_bw.BaseStream.Seek(212 + 4, SeekOrigin.Begin);
			_bw.Write(_moviDataSize + 4);

			_bw.Close();

			_alphaWriter?.Finish(averageFrameRate);
		}

		private void WriteFourCC(string fourCC)
		{
			var bytes = Encoding.ASCII.GetBytes(fourCC);
			if (bytes.Length != 4)
			{
				throw new Exception("Invalid FourCC length.");
			}
			_bw.Write(bytes);
		}

		public string Path { get; }
	}
}