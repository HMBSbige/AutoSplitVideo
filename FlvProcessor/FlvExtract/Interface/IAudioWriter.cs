namespace FlvProcessor.FlvExtract.Interface
{
	internal interface IAudioWriter
	{
		void WriteChunk(byte[] chunk, uint timeStamp);
		void Finish();
		string Path { get; }
	}
}