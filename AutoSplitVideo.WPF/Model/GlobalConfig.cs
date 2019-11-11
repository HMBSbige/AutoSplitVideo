using AutoSplitVideo.Service;
using System.IO;
using System.Text.Json;

namespace AutoSplitVideo.Model
{
	public static class GlobalConfig
	{
		public static readonly string ConfigFileName = $@"{UpdateChecker.Name}.json";

		#region Data

		public static Config Config { get; set; }

		#endregion

		#region Method

		public static void Load()
		{
			if (File.Exists(ConfigFileName))
			{
				var jsonStr = File.ReadAllText(ConfigFileName);
				try
				{
					Config = JsonSerializer.Deserialize<Config>(jsonStr);
					if (Config != null)
					{
						return;
					}
				}
				catch
				{
					// ignored
				}
			}
			Config = new Config();
		}

		public static void Save()
		{
			if (Config == null) return;
			var options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			var jsonStr = JsonSerializer.Serialize(Config, options);
			File.WriteAllTextAsync(ConfigFileName, jsonStr);
		}

		#endregion
	}
}
