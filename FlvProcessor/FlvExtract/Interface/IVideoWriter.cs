namespace FlvProcessor.FlvExtract.Interface
{
	internal interface IVideoWriter
	{
		void WriteChunk(byte[] chunk, uint timeStamp, int frameType);
		void Finish(FractionUInt32 averageFrameRate);
		string Path { get; }
	}
}