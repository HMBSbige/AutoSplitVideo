using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilibiliApi.Passport
{
	public class Passport : HttpRequest
	{
		private const string BaseUri = @"https://passport.bilibili.com";

		public static async Task<string> GetHash()
		{
			const string requestUri = @"api/oauth2/getKey";
			var pair = new Dictionary<string, string>
			{
				{@"platform", @"android"}
			};
			using var body = await GetBody(pair, true);
			return await PostAsync(BaseUri, requestUri, body);
		}

		public static async Task<string> Login(string hash, string publicKey, string username, string password)
		{
			const string requestUri = @"api/oauth2/login";
			password = Utils.RsaEncrypt(publicKey, $@"{hash}{password}");
			var pair = new Dictionary<string, string>
			{
					{@"platform", @"android"},
					{@"username", username},
					{@"password", password}
			};
			using var body = await GetBody(pair, true);
			return await PostAsync(BaseUri, requestUri, body);
		}

		public static async Task<string> GetTokenInfo(string accessToken)
		{
			var pair = new Dictionary<string, string>
			{
				{@"platform", @"android"},
				{@"access_token", accessToken},
			};
			using var body = await GetBody(pair, true);
			var para = await body.ReadAsStringAsync();
			return await GetAsync($@"https://passport.bilibili.com/api/oauth2/info?{para}");
		}
	}
}
