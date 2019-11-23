using FlvProcessor.FlvExtract.Interface;
using FlvProcessor.FlvExtract.Utils;
using System.IO;

namespace FlvProcessor.FlvExtract.VideoWriter
{
	internal class RawH264Writer : IVideoWriter
	{
		private static readonly byte[] StartCode = { 0, 0, 0, 1 };

		private FileStream _fs;
		private int _nalLengthSize;

		public RawH264Writer(string path)
		{
			Path = path;
			_fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 65536);
		}

		public void WriteChunk(byte[] chunk, uint timeStamp, int frameType)
		{
			if (chunk.Length < 4) return;

			// Reference: decode_frame from libavcodec's h264.c

			if (chunk[0] == 0)
			{ // Headers
				if (chunk.Length < 10) return;

				var offset = 8;
				_nalLengthSize = (chunk[offset++] & 0x03) + 1;
				var spsCount = chunk[offset++] & 0x1F;
				var ppsCount = -1;

				while (offset <= chunk.Length - 2)
				{
					if (spsCount == 0 && ppsCount == -1)
					{
						ppsCount = chunk[offset++];
						continue;
					}

					if (spsCount > 0) spsCount--;
					else if (ppsCount > 0) ppsCount--;
					else break;

					var len = (int)BitConverterBE.ToUInt16(chunk, offset);
					offset += 2;
					if (offset + len > chunk.Length) break;
					_fs.Write(StartCode, 0, StartCode.Length);
					_fs.Write(chunk, offset, len);
					offset += len;
				}
			}
			else
			{ // Video data
				var offset = 4;

				if (_nalLengthSize != 2)
				{
					_nalLengthSize = 4;
				}

				while (offset <= chunk.Length - _nalLengthSize)
				{
					var len = _nalLengthSize == 2 ?
							BitConverterBE.ToUInt16(chunk, offset) :
							(int)BitConverterBE.ToUInt32(chunk, offset);
					offset += _nalLengthSize;
					if (offset + len > chunk.Length) break;
					_fs.Write(StartCode, 0, StartCode.Length);
					_fs.Write(chunk, offset, len);
					offset += len;
				}
			}
		}

		public void Finish(FractionUInt32 averageFrameRate)
		{
			_fs.Close();
		}

		public string Path { get; }
	}
}