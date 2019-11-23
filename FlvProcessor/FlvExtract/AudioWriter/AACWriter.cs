using System;
using System.IO;
using FlvProcessor.FlvExtract.Interface;
using FlvProcessor.FlvExtract.Utils;

namespace FlvProcessor.FlvExtract.AudioWriter
{
	internal class AACWriter : IAudioWriter
	{
		private readonly FileStream _fs;
		private int _aacProfile;
		private int _sampleRateIndex;
		private int _channelConfig;

		public AACWriter(string path)
		{
			Path = path;
			_fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 65536);
		}

		public void WriteChunk(byte[] chunk, uint timeStamp)
		{
			if (chunk.Length < 1) return;

			if (chunk[0] == 0)
			{ // Header
				if (chunk.Length < 3) return;

				var bits = (ulong)BitConverterBE.ToUInt16(chunk, 1) << 48;

				_aacProfile = BitHelper.Read(ref bits, 5) - 1;
				_sampleRateIndex = BitHelper.Read(ref bits, 4);
				_channelConfig = BitHelper.Read(ref bits, 4);

				if (_aacProfile == 4) // HE-AAC
					_aacProfile = 1; // Uses LC profile + SBR

				if (_aacProfile < 0 || _aacProfile > 3)
					throw new Exception("Unsupported AAC profile.");
				if (_sampleRateIndex > 12)
					throw new Exception("Invalid AAC sample rate index.");
				if (_channelConfig > 6)
					throw new Exception("Invalid AAC channel configuration.");
			}
			else
			{ // Audio data
				var dataSize = chunk.Length - 1;
				ulong bits = 0;

				// Reference: WriteADTSHeader from FAAC's bitstream.c

				BitHelper.Write(ref bits, 12, 0xFFF);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 2, 0);
				BitHelper.Write(ref bits, 1, 1);
				BitHelper.Write(ref bits, 2, _aacProfile);
				BitHelper.Write(ref bits, 4, _sampleRateIndex);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 3, _channelConfig);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 13, 7 + dataSize);
				BitHelper.Write(ref bits, 11, 0x7FF);
				BitHelper.Write(ref bits, 2, 0);

				_fs.Write(BitConverterBE.GetBytes(bits), 1, 7);
				_fs.Write(chunk, 1, dataSize);
			}
		}

		public void Finish()
		{
			_fs.Close();
		}

		public string Path { get; }
	}
}