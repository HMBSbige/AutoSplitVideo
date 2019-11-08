using System;
using System.Collections.Generic;

namespace AutoSplitVideo.Model
{
	[Serializable]
	public class Config
	{
		#region Data

		public List<long> RoomList { get; set; }

		#endregion

		public Config()
		{
			RoomList = new List<long>();
		}
	}
}
