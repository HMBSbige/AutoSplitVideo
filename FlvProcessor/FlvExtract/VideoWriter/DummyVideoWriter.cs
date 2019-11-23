using FlvProcessor.FlvExtract.Interface;

namespace FlvProcessor.FlvExtract.VideoWriter
{
	internal class DummyVideoWriter : IVideoWriter
	{
		public void WriteChunk(byte[] chunk, uint timeStamp, int frameType)
		{
		}

		public void Finish(FractionUInt32 averageFrameRate)
		{
		}

		public string Path => null;
	}
}