namespace BilibiliApi.Event
{
	public delegate void LogEvent(object sender, LogEventArgs e);

	public class LogEventArgs
	{
		public string Log;
	}
}
