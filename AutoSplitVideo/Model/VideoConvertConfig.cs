namespace AutoSplitVideo.Model
{
	internal class VideoConvertConfig
	{
		public bool DeleteFlv;
		public bool OnlyConvert;
		public bool IsSkipSameMp4;
		public bool IsSendToRecycleBin;
		public bool OutputSameAsInput;
		public bool UseFlvFix;

		public VideoConvertConfig()
		{
			DeleteFlv = true;
			OnlyConvert = false;
			IsSkipSameMp4 = true;
			IsSendToRecycleBin = true;
			OutputSameAsInput = true;
			UseFlvFix = false;
		}
	}
}
