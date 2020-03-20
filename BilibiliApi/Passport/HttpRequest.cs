using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BilibiliApi.Passport
{
	public abstract class HttpRequest
	{
		private const string DefaultUserAgent = @"Mozilla/5.0 BiliComic/2.14.0";
		private const int DefaultTimeout = 10000;

		private const string AppSecret = @"59b43e04ad6965f34319062b478f83dd";
		private const string Appkey = @"4409e2ce8ffd12b8";

		protected static async Task<string> GetAsync(string url)
		{
			var httpClient = new HttpClient
			{
				Timeout = TimeSpan.FromMilliseconds(DefaultTimeout)
			};
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add(@"User-Agent", DefaultUserAgent);

			var response = await httpClient.SendAsync(request);

			response.EnsureSuccessStatusCode();
			var resultStr = await response.Content.ReadAsStringAsync();
			return resultStr;
		}

		protected static async Task<string> PostAsync(string baseUri, string requestUri, FormUrlEncodedContent content)
		{
			using var httpClient = new HttpClient
			{
				BaseAddress = new Uri(baseUri),
				Timeout = TimeSpan.FromMilliseconds(DefaultTimeout)
			};
			httpClient.DefaultRequestHeaders.Add(@"User-Agent", DefaultUserAgent);
			var response = await httpClient.PostAsync(requestUri, content);
			var resultContent = await response.Content.ReadAsStringAsync();
			return resultContent;
		}

		protected static async Task<FormUrlEncodedContent> GetBody(Dictionary<string, string> pair, bool isSign)
		{
			if (isSign)
			{
				pair[@"appkey"] = Appkey;
				pair = pair.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
				using var temp = new FormUrlEncodedContent(pair);
				var str = await temp.ReadAsStringAsync();
				var md5 = Utils.Md5($@"{str}{AppSecret}");
				pair.Add(@"sign", md5);
			}

			return new FormUrlEncodedContent(pair);
		}
	}
}
