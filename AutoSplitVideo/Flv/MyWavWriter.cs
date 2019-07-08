namespace AutoSplitVideo.Flv
{
	internal class MyWavWriter : IAudioWriter
	{
		private WavWriter _wr;
		private int blockAlign;

		public MyWavWriter(string path, int bitsPerSample, int channelCount, int sampleRate)
		{
			Path = path;
			_wr = new WavWriter(path, bitsPerSample, channelCount, sampleRate);
			blockAlign = bitsPerSample / 8 * channelCount;
		}

		public void WriteChunk(byte[] chunk, uint timeStamp)
		{
			_wr.Write(chunk, chunk.Length / blockAlign);
		}

		public void Finish()
		{
			_wr.Close();
		}

		public string Path { get; }
	}
}