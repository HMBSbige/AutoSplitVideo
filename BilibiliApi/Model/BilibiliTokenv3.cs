using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BilibiliApi.Model
{
	public class BilibiliTokenv3: BilibiliToken
	{
		public BilibiliCookie Cookie;

		public BilibiliTokenv3(string json)
		{
			Parse(json);
		}

		public new void Parse(string json)
		{
			Code = -1;
			using var document = JsonDocument.Parse(json);
			var root = document.RootElement;
			if (root.TryGetProperty(@"code", out var codeProperty) && codeProperty.TryGetInt32(out Code) && Code == 0)
			{
				if (root.TryGetProperty(@"ts", out var tsProperty))
				{
					var baseTime = Utils.GetTime(tsProperty.GetRawText());
					if (root.TryGetProperty(@"data", out var data))
					{
						if (data.TryGetProperty(@"token_info", out var tokenInfo))
						{
							if (tokenInfo.TryGetProperty(@"mid", out var mid) && mid.TryGetInt64(out Uid) &&
							tokenInfo.TryGetProperty(@"access_token", out var accessToken) &&
							tokenInfo.TryGetProperty(@"expires_in", out var expiresInProperty) && expiresInProperty.TryGetInt64(out var expiresIn))
							{
								AccessToken = accessToken.GetString();
								Expires = baseTime + TimeSpan.FromSeconds(expiresIn);
							}
							if (tokenInfo.TryGetProperty(@"refresh_token", out var refreshToken))
							{
								RefreshToken = refreshToken.GetString();
							}
						}
						if (data.TryGetProperty(@"cookie_info", out var cookieInfo))
						{
							if (cookieInfo.TryGetProperty(@"cookies", out var cookies) && cookies.ValueKind == JsonValueKind.Array)
							{
								var temp = new Dictionary<string, string>();
								foreach (var cookie in cookies.EnumerateArray())
								{
									if (cookie.TryGetProperty(@"name", out var name) &&
										cookie.TryGetProperty(@"value", out var value)
									)
									{
										temp[name.GetString()] = value.GetString();
									}
								}
								Cookie = new BilibiliCookie(temp);
							}
						}
					}
				}
			}
		}

	}
}
