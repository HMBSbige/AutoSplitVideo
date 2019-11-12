using AutoSplitVideo.Model;
using AutoSplitVideo.Service;
using AutoSplitVideo.ViewModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

		private void ShowHideMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MainWindowViewModel.TriggerShowHide();
		}

		private void SelectDirectoryButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonOpenFileDialog
			{
				IsFolderPicker = true,
				Multiselect = false,
				Title = @"选择存储目录",
				AddToMostRecentlyUsedList = false,
				EnsurePathExists = true,
				NavigateToShortcut = true,
				InitialDirectory = MainWindowViewModel.CurrentConfig.RecordDirectory
			};
			if (dlg.ShowDialog(this) == CommonFileDialogResult.Ok)
			{
				MainWindowViewModel.CurrentConfig.RecordDirectory = dlg.FileName;
			}
		}

		private void OpenDirectoryButton_OnClick(object sender, RoutedEventArgs e)
		{
			Utils.Utils.OpenDir(MainWindowViewModel.CurrentConfig.RecordDirectory);
		}

		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			MainWindowViewModel.StopGetDiskUsage();
			MainWindowViewModel.StopAllMonitors();
		}

		#region ToolBar hide grip Hack

		private void ToolBar_OnLoaded(object sender, RoutedEventArgs e)
		{
			if (sender is ToolBar toolBar)
			{
				// Hide grip
				if (toolBar.Template.FindName(@"OverflowGrid", toolBar) is FrameworkElement overflowGrid)
				{
					overflowGrid.Visibility = Visibility.Collapsed;
				}

				if (toolBar.Template.FindName(@"MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
				{
					mainPanelBorder.Margin = new Thickness();
				}
			}
		}

		#endregion

		private void AddRoomTextBox_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				AddRoomButton_OnClick(sender, e);
			}
		}

		private async void AddRoomButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (int.TryParse(AddRoomTextBox.Text.Trim(), out var roomId) && roomId > 0)
			{
				if (await MainWindowViewModel.AddRoom(roomId))
				{
					AddRoomTextBox.Text = string.Empty;
					return;
				}
			}

			MessageBox.Show(@"添加失败", UpdateChecker.Name, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void RemoveRoomButton_OnClick(object sender, RoutedEventArgs e)
		{
			var removeRooms = GetSelectIds();
			if (removeRooms.Count == 0) return;
			var str = string.Join(',', removeRooms);
			if (MessageBox.Show($@"确定移除：{str}？", UpdateChecker.Name, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
			{
				MainWindowViewModel.RemoveRoom(removeRooms);
			}
		}

		private void ClearLogButton_OnClick(object sender, RoutedEventArgs e)
		{
			Log.ClearLog();
		}

		private void OpenLogButton_OnClick(object sender, RoutedEventArgs e)
		{
			Utils.Utils.OpenFile(Log.LogFileName);
		}

		private void ClearTitleFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			TitleLog.ClearLog();
		}

		private void OpenTitleFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			Utils.Utils.OpenFile(TitleLog.TitleFileName);
		}

		private List<int> GetSelectIds()
		{
			var rooms = new List<int>();
			foreach (var item in DataGrid.SelectedItems)
			{
				if (item is RoomSetting setting)
				{
					rooms.Add(setting.RoomId);
				}
			}
			return rooms;
		}

		private void ManualRefreshMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var rooms = GetSelectIds();
			if (rooms.Count == 0) return;
			MainWindowViewModel.ManualRefresh(rooms);
		}

		private void OpenRoomDirMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (DataGrid.SelectedItem is RoomSetting setting)
			{
				Utils.Utils.OpenDir(Path.Combine(GlobalConfig.Config.RecordDirectory, $@"{setting.RoomId}"));
			}
		}

		private void ClearLogMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MainWindowViewModel.Logs.Clear();
		}
	}
}
