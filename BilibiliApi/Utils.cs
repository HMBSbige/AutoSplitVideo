namespace BilibiliApi
{
	public static class Utils
	{
		internal static string UserAgent
		{
			get
			{
				var version = typeof(Utils).Assembly.GetName().Version.ToString();
				return $"Mozilla/5.0 AutoSplitVideo/{version} (+https://github.com/HMBSbige/AutoSplitVideo)";
			}
		}
	}
}
