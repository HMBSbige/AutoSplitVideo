using AutoSplitVideo.Model;
using AutoSplitVideo.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace AutoSplitVideo.View
{
	public partial class MainWindow
	{
		public MainWindowViewModel MainWindowViewModel { get; set; } = new MainWindowViewModel();

		public MainWindow()
		{
			InitializeComponent();
			MainWindowViewModel.Window = this;
		}

		private CloseReason _closeReason = CloseReason.Unknown;

		private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			_closeReason = CloseReason.Unknown;
			Close();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			#region CloseReasonHack

			if (PresentationSource.FromDependencyObject(this) is HwndSource source)
			{
				source.AddHook(WindowProc);
			}

			#endregion

		}

		#region CloseReasonHack

		private void MainWindow_OnClosing(object sender, CancelEventArgs e)
		{
			switch (_closeReason)
			{
				case CloseReason.Unknown:
				case CloseReason.Logoff:
					break;
				case CloseReason.User:
				{
					MainWindowViewModel.HideWindow();
					e.Cancel = true;
					break;
				}
			}
		}

		private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case 0x11:
				case 0x16:
					_closeReason = CloseReason.Logoff;
					break;
				case 0x112:
					if (((ushort)wParam & 0xfff0) == 0xf060)
					{
						_closeReason = CloseReason.User;
					}
					break;
			}
			return IntPtr.Zero;
		}

		#endregion
	}
}
