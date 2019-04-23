namespace AutoSplitVideo.Model
{
	internal class VideoConvertConfig
	{
		public bool DeleteFlv;
		public bool OnlyConvert;
		public bool IsSkipSameMp4;
		public bool IsSendToRecycleBin;
		public bool OutputSameAsInput;

		public VideoConvertConfig(bool deleteFlv, bool onlyConvert, bool isSkipSameMp4, bool isSendToRecycleBin, bool outputSameAsInput)
		{
			DeleteFlv = deleteFlv;
			OnlyConvert = onlyConvert;
			IsSkipSameMp4 = isSkipSameMp4;
			IsSendToRecycleBin = isSendToRecycleBin;
			OutputSameAsInput = outputSameAsInput;
		}

		public VideoConvertConfig()
		{
			DeleteFlv = true;
			OnlyConvert = false;
			IsSkipSameMp4 = true;
			IsSendToRecycleBin = true;
			OutputSameAsInput = true;
		}
	}
}
