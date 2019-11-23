using FlvProcessor.FlvExtract.Interface;

namespace FlvProcessor.FlvExtract.AudioWriter
{
	internal class MyWavWriter : IAudioWriter
	{
		private readonly WavWriter _wr;
		private readonly int _blockAlign;

		public MyWavWriter(string path, int bitsPerSample, int channelCount, int sampleRate)
		{
			Path = path;
			_wr = new WavWriter(path, bitsPerSample, channelCount, sampleRate);
			_blockAlign = bitsPerSample / 8 * channelCount;
		}

		public void WriteChunk(byte[] chunk, uint timeStamp)
		{
			_wr.Write(chunk, chunk.Length / _blockAlign);
		}

		public void Finish()
		{
			_wr.Close();
		}

		public string Path { get; }
	}
}