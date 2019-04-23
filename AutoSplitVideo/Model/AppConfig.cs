using AutoSplitVideo.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;

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
		public int IsAutoConvertFlv;

		public int DeleteFlv;
		public int OnlyConvert;
		public int IsSkipSameMp4;
		public int IsSendToRecycleBin;
		public int OutputSameAsInput;

		public decimal N1;
		public decimal N2;

		#endregion

		public AppConfig(string path)
		{
			_configPath = path;
			TableIndex = 0;
			OutputPath = string.Empty;
			StreamUrlIndex = 0;
			Rooms = new List<long>();
			IsNotify = 0;
			IsAutoConvertFlv = 1;

			DeleteFlv = 1;
			OnlyConvert = 0;
			IsSkipSameMp4 = 1;
			IsSendToRecycleBin = 1;
			OutputSameAsInput = 1;

			N1 = new decimal(8.000);
			N2 = new decimal(180);
		}

		private void Write(string section, string key, string value)
		{
			Config.WriteString(section, key, value, _configPath);
		}

		private void WriteGeneral(string key, string value)
		{
			Write(@"General", key, value);
		}

		private void WriteVideoConvert(string key, string value)
		{
			Write(@"VideoConvert", key, value);
		}

		private string Read(string section, string key, string def)
		{
			return Config.ReadString(section, key, def, _configPath);
		}

		private string ReadGeneral(string key, string def = @"")
		{
			return Read(@"General", key, def);
		}

		private string ReadVideoConvert(string key, string def = @"")
		{
			return Read(@"VideoConvert", key, def);
		}

		private int ParseBool_General(string name, int def)
		{
			if (int.TryParse(ReadGeneral(name, Convert.ToString(def)), out var b))
			{
				if (b != def)
				{
					return def == 1 ? 0 : 1;
				}
			}
			return def;
		}

		private int ParseBool_VideoConvert(string name, int def)
		{
			if (int.TryParse(ReadVideoConvert(name, Convert.ToString(def)), out var b))
			{
				if (b != def)
				{
					return def == 1 ? 0 : 1;
				}
			}

			return def;
		}

		public void Save()
		{
			WriteGeneral(nameof(TableIndex), TableIndex.ToString());
			WriteGeneral(nameof(OutputPath), OutputPath);
			WriteGeneral(nameof(StreamUrlIndex), StreamUrlIndex.ToString());
			WriteGeneral(nameof(Rooms), Rooms.ToStr());
			WriteGeneral(nameof(IsNotify), IsNotify.ToString());
			WriteGeneral(nameof(IsAutoConvertFlv), IsAutoConvertFlv.ToString());

			WriteVideoConvert(nameof(DeleteFlv), DeleteFlv.ToString());
			WriteVideoConvert(nameof(OnlyConvert), OnlyConvert.ToString());
			WriteVideoConvert(nameof(IsSkipSameMp4), IsSkipSameMp4.ToString());
			WriteVideoConvert(nameof(IsSendToRecycleBin), IsSendToRecycleBin.ToString());
			WriteVideoConvert(nameof(OutputSameAsInput), OutputSameAsInput.ToString());

			WriteVideoConvert(nameof(N1), N1.ToString(CultureInfo.InvariantCulture));
			WriteVideoConvert(nameof(N2), N2.ToString(CultureInfo.InvariantCulture));
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

			IsNotify = ParseBool_General(nameof(IsNotify), 0);
			IsAutoConvertFlv = ParseBool_General(nameof(IsAutoConvertFlv), 1);


			DeleteFlv = ParseBool_VideoConvert(nameof(DeleteFlv), 1);
			OnlyConvert = ParseBool_VideoConvert(nameof(OnlyConvert), 0);
			IsSkipSameMp4 = ParseBool_VideoConvert(nameof(IsSkipSameMp4), 1);
			IsSendToRecycleBin = ParseBool_VideoConvert(nameof(IsSendToRecycleBin), 1);
			OutputSameAsInput = ParseBool_VideoConvert(nameof(OutputSameAsInput), 1);

			if (decimal.TryParse(ReadVideoConvert(nameof(N1), @"8.000"), out var n1))
			{
				N1 = n1;
			}

			if (decimal.TryParse(ReadVideoConvert(nameof(N2), @"180"), out var n2))
			{
				N2 = n2;
			}
		}
	}
}
