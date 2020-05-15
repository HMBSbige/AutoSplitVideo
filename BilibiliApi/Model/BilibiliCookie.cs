using System;
using System.Collections.Generic;

namespace BilibiliApi.Model
{
	public class BilibiliCookie
	{
		public string bili_jct = string.Empty;
		public string DedeUserID = string.Empty;
		public string DedeUserID__ckMd5 = string.Empty;
		public string sid = string.Empty;
		public string SESSDATA = string.Empty;

		public BilibiliCookie(string str)
		{
			Parse(str);
		}

		public BilibiliCookie(Dictionary<string, string> cookies)
		{
			Parse(cookies);
		}

		public void Parse(string str)
		{
			if (string.IsNullOrWhiteSpace(str)) return;

			var array = str.Split(';', StringSplitOptions.RemoveEmptyEntries);

			var cookies = new Dictionary<string, string>();

			foreach (var s in array)
			{
				var pair = s.Trim().Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
				if (pair.Length == 2)
				{
					cookies[pair[0]] = pair[1];
				}
			}

			Parse(cookies);
		}

		public void Parse(Dictionary<string, string> cookies)
		{
			cookies.TryGetValue(@"bili_jct", out bili_jct);
			cookies.TryGetValue(@"DedeUserID", out DedeUserID);
			cookies.TryGetValue(@"DedeUserID__ckMd5", out DedeUserID__ckMd5);
			cookies.TryGetValue(@"sid", out sid);
			cookies.TryGetValue(@"SESSDATA", out SESSDATA);
		}

		public override string ToString()
		{
			var array = new List<string>();

			if (!string.IsNullOrWhiteSpace(bili_jct))
			{
				array.Add($@"bili_jct={bili_jct}");
			}

			if (!string.IsNullOrWhiteSpace(DedeUserID))
			{
				array.Add($@"DedeUserID={DedeUserID}");
			}

			if (!string.IsNullOrWhiteSpace(DedeUserID__ckMd5))
			{
				array.Add($@"DedeUserID__ckMd5={DedeUserID__ckMd5}");
			}

			if (!string.IsNullOrWhiteSpace(sid))
			{
				array.Add($@"sid={sid}");
			}

			if (!string.IsNullOrWhiteSpace(SESSDATA))
			{
				array.Add($@"SESSDATA={SESSDATA}");
			}

			return string.Join(';', array);
		}
	}
}
