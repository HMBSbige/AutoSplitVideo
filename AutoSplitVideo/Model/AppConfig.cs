using AutoSplitVideo.Utils;
using System;
using System.Collections.Generic;

namespace AutoSplitVideo.Model
{
	public class AppConfig
	{
		private readonly string _configPath;

		#region General

		public int TableIndex;
		public string OutputPath;
		public int StreamUrlIndex;
		public IEnumerable<long> Rooms;
		public int IsNotify;

		#endregion

		public AppConfig(string path)
		{
			_configPath = path;
			TableIndex = 0;
			OutputPath = string.Empty;
			StreamUrlIndex = 0;
			Rooms = new List<long>();
			IsNotify = 0;
		}

		private void Write(string section, string key, string value)
		{
			Config.WriteString(section, key, value, _configPath);
		}

		private void WriteGeneral(string key, string value)
		{
			Write(@"General", key, value);
		}

		private string Read(string section, string key, string def)
		{
			return Config.ReadString(section, key, def, _configPath);
		}

		private string ReadGeneral(string key, string def = @"")
		{
			return Read(@"General", key, def);
		}

		public void Save()
		{
			WriteGeneral(nameof(TableIndex), TableIndex.ToString());
			WriteGeneral(nameof(OutputPath), OutputPath);
			WriteGeneral(nameof(StreamUrlIndex), StreamUrlIndex.ToString());
			WriteGeneral(nameof(Rooms), Rooms.ToStr());
			WriteGeneral(nameof(IsNotify), IsNotify.ToString());
		}

		public void Load()
		{
			if (int.TryParse(ReadGeneral(nameof(TableIndex), Convert.ToString(0)), out var index))
			{
				if (index >= 0 && index < 3)
				{
					TableIndex = index;
				}
			}

			OutputPath = ReadGeneral(nameof(OutputPath));

			if (int.TryParse(ReadGeneral(nameof(StreamUrlIndex), Convert.ToString(0)), out var urlIndex))
			{
				if (urlIndex >= 0 && urlIndex < 4)
				{
					StreamUrlIndex = urlIndex;
				}
			}

			Rooms = ReadGeneral(nameof(Rooms)).ToListInt();

			if (int.TryParse(ReadGeneral(nameof(IsNotify), Convert.ToString(0)), out var isNotify))
			{
				if (isNotify != 0)
				{
					IsNotify = 1;
				}
			}
		}

	}
}
