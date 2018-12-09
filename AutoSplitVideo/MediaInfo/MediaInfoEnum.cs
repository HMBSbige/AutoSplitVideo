namespace AutoSplitVideo.MediaInfo
{
	public enum StreamKind
	{
		General,
		Video,
		Audio,
		Text,
		Other,
		Image,
		Menu
	}

	public enum InfoKind
	{
		Name,
		Text,
		Measure,
		Options,
		NameText,
		MeasureText,
		Info,
		HowTo
	}

	public enum InfoOptions
	{
		ShowInInform,
		Support,
		ShowInSupported,
		TypeOfValue
	}

	public enum InfoFileOptions
	{
		FileOptionNothing = 0x00,
		FileOptionNoRecursive = 0x01,
		FileOptionCloseAll = 0x02,
		FileOptionMax = 0x04
	}

	public enum Status
	{
		None = 0x00,
		Accepted = 0x01,
		Filled = 0x02,
		Updated = 0x04,
		Finalized = 0x08
	}
}
