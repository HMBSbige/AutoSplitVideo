using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoSplitVideo.Utils
{
	public static class ViewUtils
	{
		public static void ShowWindow(this Window window, bool notClosing = true)
		{
			if (notClosing)
			{
				window.Visibility = Visibility.Visible;
			}

			Win32.UnMinimize(window);
			if (!window.Topmost)
			{
				window.Topmost = true;
				window.Topmost = false;
			}
		}

		public static bool IsScrolledToEnd(this TextBox textBox)
		{
			return Math.Abs(textBox.VerticalOffset + textBox.ViewportHeight - textBox.ExtentHeight) < 0.001;
		}
	}
}
