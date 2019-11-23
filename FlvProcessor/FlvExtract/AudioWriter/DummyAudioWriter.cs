using FlvProcessor.FlvExtract.Interface;

namespace FlvProcessor.FlvExtract.AudioWriter
{
	internal class DummyAudioWriter : IAudioWriter
	{
		public void WriteChunk(byte[] chunk, uint timeStamp)
		{
		}

		public void Finish()
		{
		}

		public string Path => null;
	}
}