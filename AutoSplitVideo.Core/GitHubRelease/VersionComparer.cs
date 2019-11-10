using System.Collections.Generic;

namespace AutoSplitVideo.Core.GitHubRelease
{
	public class VersionComparer : IComparer<object>
	{
		public int Compare(object x, object y)
		{
			return VersionUtil.CompareVersion(x?.ToString(), y?.ToString());
		}
	}
}
