using Kyozy.MiniblinkNet;
using System;
using System.Collections.Generic;
using System.Windows;

namespace AutoSplitVideo.View
{
	public partial class ChromeWindow
	{
		private readonly WebView _mWke;

		public ChromeWindow(Dictionary<string, string> cookies)
		{
			InitializeComponent();
			try
			{
				_mWke = new WebView();
				_mWke.Bind(PictureBox);
				_mWke.OnLoadingFinish += M_wke_OnLoadingFinish;

				Title = $@"{_mWke.Name} {_mWke.GetUserAgent()}";

				ClearCookie();
				_cookies = cookies;

				_mWke.LoadURL(@"https://passport.bilibili.com/login");
			}
			catch
			{
				_mWke = null;
				MessageBox.Show(@"加载失败");
				Loaded += (o, e) => { DialogResult = false; };
			}
		}

		private const string Domain = @".bilibili.com";

		private readonly Dictionary<string, string> _cookies;

		private void ClearCookie()
		{
			_mWke.PerformCookieCommand(wkeCookieCommand.ClearAllCookies);
		}

		private void GetCookie()
		{
			_cookies.Clear();
			var visitor = new wkeCookieVisitor(
			(IntPtr userData, string name, string value, string domain, string path, int secure, int httpOnly, ref int expires) =>
			{
				if (domain == Domain)
				{
					_cookies[name] = value;
				}

				return false;
			});
			_mWke.VisitAllCookie(visitor, IntPtr.Zero);
			CheckCookie();
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

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			GetCookie();
		}

		private void M_wke_OnLoadingFinish(object sender, LoadingFinishEventArgs e)
		{
			GetCookie();
		}

		private void ChromeWindow_OnClosed(object sender, EventArgs e)
		{
			_mWke?.Dispose();
		}
	}
}
