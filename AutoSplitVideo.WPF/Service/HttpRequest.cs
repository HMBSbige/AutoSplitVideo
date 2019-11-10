using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutoSplitVideo.Service
{
	public abstract class HttpRequest
	{
		private const string DefaultUserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.97 Safari/537.36";
		private const int DefaultGetTimeout = 30000;

		protected static async Task<string> GetAsync(string url, bool proxy = true, string userAgent = @"", double timeout = DefaultGetTimeout)
		{
			var httpClientHandler = new HttpClientHandler
			{
				UseProxy = proxy
			};
			var httpClient = new HttpClient(httpClientHandler)
			{
				Timeout = TimeSpan.FromMilliseconds(timeout)
			};
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add(@"User-Agent", string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent);

			var response = await httpClient.SendAsync(request);

			response.EnsureSuccessStatusCode();
			var resultStr = await response.Content.ReadAsStringAsync();
			return resultStr;
		}
	}
}
