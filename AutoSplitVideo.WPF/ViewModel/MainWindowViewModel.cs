using AutoSplitVideo.Utils;
using AutoSplitVideo.View;
using System.Windows;

namespace AutoSplitVideo.ViewModel
{
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			Window = null;
		}

		#region Window

		public MainWindow Window { private get; set; }

		public void HideWindow()
		{
			if (Window == null)
			{
				return;
			}
			Window.Visibility = Visibility.Hidden;
		}

		public void ShowWindow(bool notClosing = true)
		{
			Window?.ShowWindow(notClosing);
		}

		public void TriggerShowHide()
		{
			if (Window == null)
			{
				return;
			}

			if (Window.Visibility == Visibility.Visible)
			{
				HideWindow();
			}
			else
			{
				ShowWindow();
			}
		}

		#endregion
	}
}
