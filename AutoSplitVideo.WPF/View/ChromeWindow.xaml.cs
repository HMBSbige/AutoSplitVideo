using CefSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AutoSplitVideo.View
{
	public partial class ChromeWindow
	{
		public ChromeWindow(Dictionary<string, string> cookies)
		{
			InitializeComponent();
			Title = $@"Chrome {Cef.ChromiumVersion}";
			ClearCookie().Wait();
			_cookies = cookies;
		}

		private const string Url = @"https://live.bilibili.com/";
		private const string Domain = @".bilibili.com";

		private readonly Dictionary<string, string> _cookies;

		private static async Task ClearCookie()
		{
			var cookieManager = Cef.GetGlobalCookieManager();
			await cookieManager.DeleteCookiesAsync().ConfigureAwait(false);
		}

		private async Task GetCookie()
		{
			var cookieManager = Cef.GetGlobalCookieManager();

			var l = await cookieManager.VisitUrlCookiesAsync(Url, true).ConfigureAwait(false);
			if (l != null)
			{
				_cookies.Clear();
				foreach (var cookie in l.Where(cookie => cookie.Domain == Domain))
				{
					_cookies[cookie.Name] = cookie.Value;
				}
				CheckCookie();
			}
		}

		private void CheckCookie()
		{
			if (_cookies.ContainsKey(@"bili_jct") &&
				_cookies.ContainsKey(@"DedeUserID") &&
				_cookies.ContainsKey(@"DedeUserID__ckMd5") &&
				_cookies.ContainsKey(@"sid") &&
				_cookies.ContainsKey(@"SESSDATA"))
			{
				Dispatcher.InvokeAsync(() => { DialogResult = true; });
			}
		}

		private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			await GetCookie();
		}

		private async void Browser_OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
		{
			await GetCookie();
		}
	}
}
