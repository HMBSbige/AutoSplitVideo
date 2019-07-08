namespace AutoSplitVideo.Flv
{
	internal class DummyVideoWriter : IVideoWriter
	{
		public DummyVideoWriter()
		{
		}

		public void WriteChunk(byte[] chunk, uint timeStamp, int frameType)
		{
		}

		public void Finish(FractionUInt32 averageFrameRate)
		{
		}

		public string Path => null;
	}
}