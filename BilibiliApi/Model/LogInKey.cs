using System.Text.Json;

namespace BilibiliApi.Model
{
	public class LogInKey
	{
		public string Hash = @"";
		public string Key = @"";
		public int Code = -1;

		public LogInKey(string json)
		{
			using var document = JsonDocument.Parse(json);
			var root = document.RootElement;
			if (root.TryGetProperty(@"code", out var codeProperty) && codeProperty.TryGetInt32(out Code) && Code == 0)
			{
				if (root.TryGetProperty(@"data", out var data))
				{
					if (data.TryGetProperty(@"hash", out var hash) && data.TryGetProperty(@"key", out var key))
					{
						Hash = hash.GetString();
						Key = key.GetString();
					}
				}
			}
		}
	}
}
