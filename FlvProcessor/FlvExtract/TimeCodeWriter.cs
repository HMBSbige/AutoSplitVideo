using System.IO;
using System.Text;

namespace FlvProcessor.FlvExtract
{
	internal class TimeCodeWriter
	{
		private StreamWriter _sw;

		public TimeCodeWriter(string path)
		{
			Path = path;
			if (path != null)
			{
				_sw = new StreamWriter(path, false, Encoding.ASCII);
				_sw.WriteLine("# timecode format v2");
			}
		}

		public void Write(uint timeStamp)
		{
			_sw?.WriteLine(timeStamp);
		}

		public void Finish()
		{
			if (_sw != null)
			{
				_sw.Close();
				_sw = null;
			}
		}

		public string Path { get; }
	}
}