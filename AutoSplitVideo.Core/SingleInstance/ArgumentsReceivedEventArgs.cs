using System;

namespace AutoSplitVideo.Core.SingleInstance
{
	public class ArgumentsReceivedEventArgs : EventArgs
	{
		public string[] Args { get; set; }
	}
}
