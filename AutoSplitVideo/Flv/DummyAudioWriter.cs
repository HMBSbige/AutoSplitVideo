namespace AutoSplitVideo.Flv
{
	internal class DummyAudioWriter : IAudioWriter
	{
		public DummyAudioWriter()
		{
		}

		public void WriteChunk(byte[] chunk, uint timeStamp)
		{
		}

		public void Finish()
		{
		}

		public string Path => null;
	}
}