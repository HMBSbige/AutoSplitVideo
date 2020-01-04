using AutoSplitVideo.Model;
using AutoSplitVideo.Service;
using AutoSplitVideo.Utils;
using AutoSplitVideo.ViewModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
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

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			#region CloseReasonHack

			if (PresentationSource.FromDependencyObject(this) is HwndSource source)
			{
				source.AddHook(WindowProc);
			}

			#endregion

			AutoStartupCheckBox.IsChecked = AutoStartup.Check();
		}

		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			MainWindowViewModel.StopGetDiskUsage();
			MainWindowViewModel.StopAllMonitors();
			MainWindowViewModel.StopAllVideoConvert();
		}

		#region CloseReasonHack

		private CloseReason _closeReason = CloseReason.Unknown;

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

		#region 通知栏图标右键菜单

		private void ShowHideMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MainWindowViewModel.TriggerShowHide();
		}

		private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			Task.Run(() =>
			{
				if (MessageBox.Show(@"确定退出？", UpdateChecker.Name,
							MessageBoxButton.OKCancel, MessageBoxImage.Question)
					!= MessageBoxResult.OK)
				{
					return;
				}

				_closeReason = CloseReason.Unknown;
				Dispatcher?.InvokeAsync(Close);
			});
		}

		#endregion

		#region 设置

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

		private void AutoStartupCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (!AutoStartup.Set(AutoStartupCheckBox.IsChecked.GetValueOrDefault()))
			{
				MessageBox.Show(@"设置失败", @"错误", MessageBoxButton.OK, MessageBoxImage.Error);
				AutoStartupCheckBox.IsChecked = !AutoStartupCheckBox.IsChecked;
			}
		}
		#endregion

		#region 日志

		private void LogTextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox logTextBox)
			{
				if (logTextBox.SelectionStart == 0 || logTextBox.IsScrolledToEnd())
				{
					logTextBox.ScrollToEnd();
				}
			}
		}

		private void ClearLogMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MainWindowViewModel.Logs.Clear();
		}

		#endregion

		#region 录播机

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
			if (MessageBox.Show($@"确定移除：{str}？", UpdateChecker.Name, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
			{
				MainWindowViewModel.RemoveRoom(removeRooms);
			}
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

		private void OpenRoomMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (DataGrid.SelectedItem is RoomSetting setting)
			{
				Utils.Utils.OpenUrl($@"https://live.bilibili.com/{setting.RoomId}");
			}
		}

		#endregion

		#region 拖拽

		private void UIElement_OnPreviewDragOver(object sender, DragEventArgs e)
		{
			if (sender is TextBox)
			{
				e.Handled = true;
				return;
			}

			if (sender is TabItem tabItem)
			{
				TabControl.SelectedItem = tabItem;
			}
		}

		private void TextBox_OnPreviewDrop(object sender, DragEventArgs e)
		{
			if (!(sender is TextBox textBox)) return;
			var path = ((Array)e.Data.GetData(DataFormats.FileDrop))?.GetValue(0).ToString();
			if (path != null && File.Exists(path))
			{
				path = Path.GetFullPath(path);
				textBox.Text = path;

				e.Handled = true;
			}
		}

		private void TextBox2_OnPreviewDrop(object sender, DragEventArgs e)
		{
			if (!(sender is TextBox textBox)) return;
			var path = ((Array)e.Data.GetData(DataFormats.FileDrop))?.GetValue(0).ToString();
			if (path != null && (File.Exists(path) || Directory.Exists(path)))
			{
				path = Path.GetFullPath(path);
				textBox.Text = path;

				e.Handled = true;
			}
		}

		#endregion

		#region FLV

		private void GetOutputPath(string path, string oldPath)
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				OutputFileTextBox.Text = string.Empty;
				return;
			}

			var ext = Path.GetExtension(path);
			var oldName = Path.ChangeExtension(path, null);
			for (var i = 1; i < int.MaxValue; ++i)
			{
				var newPath = $@"{oldName}_{i}{ext}";
				if (!File.Exists(newPath) && newPath != oldPath)
				{
					OutputFileTextBox.Text = newPath;
					break;
				}
			}
		}

		private void InputFileTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (!(sender is TextBox textBox)) return;
			var path = textBox.Text;
			GetOutputPath(path, null);
		}

		private void InputFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonOpenFileDialog
			{
				IsFolderPicker = false,
				Multiselect = false,
				Title = @"打开",
				AddToMostRecentlyUsedList = false,
				EnsurePathExists = true,
				NavigateToShortcut = true,
				Filters =
				{
					new CommonFileDialogFilter(@"视频文件",@"*.mp4;*.flv;*.mkv"),
					new CommonFileDialogFilter(@"所有文件",@"*.*")
				}
			};
			if (dlg.ShowDialog(this) == CommonFileDialogResult.Ok)
			{
				InputFileTextBox.Text = dlg.FileName;
			}
		}

		private void OutputButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonSaveFileDialog
			{
				Title = @"另存为",
				AddToMostRecentlyUsedList = false,
				NavigateToShortcut = true,
				DefaultFileName = Path.GetFileName(OutputFileTextBox.Text),
				DefaultExtension = Path.GetExtension(OutputFileTextBox.Text),
				DefaultDirectory = Path.GetDirectoryName(OutputFileTextBox.Text),
				Filters =
				{
					new CommonFileDialogFilter(@"视频文件", @"*.mp4;*.flv;*.mkv"),
					new CommonFileDialogFilter(@"所有文件", @"*.*")
				}
			};
			if (dlg.ShowDialog(this) == CommonFileDialogResult.Ok)
			{
				OutputFileTextBox.Text = dlg.FileName;
			}
		}

		private void ClipButton_OnClick(object sender, RoutedEventArgs e)
		{
			var inputPath = InputFileTextBox.Text;
			var outputPath = OutputFileTextBox.Text;
			var startTime = StartTimeControl.Text;
			var duration = DurationControl.Text;
			MainWindowViewModel.AddSplitTask(inputPath, outputPath, startTime, duration);
			GetOutputPath(inputPath, outputPath);
		}

		private void InputFileTextBox2_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (!(sender is TextBox textBox)) return;
			var path = textBox.Text;
			if (Directory.Exists(path))
			{
				OutputFileTextBox2.Text = path;
				return;
			}

			if (File.Exists(path) && Path.GetExtension(path) != @".mp4")
			{
				var newPath = Path.ChangeExtension(path, @"mp4");
				if (!File.Exists(newPath))
				{
					OutputFileTextBox2.Text = newPath;
				}
			}
		}

		private void SelectButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonOpenFileDialog
			{
				IsFolderPicker = false,
				Multiselect = false,
				Title = @"打开",
				AddToMostRecentlyUsedList = false,
				EnsurePathExists = true,
				NavigateToShortcut = true,
				Filters =
				{
						new CommonFileDialogFilter(@"视频文件", @"*.mp4;*.flv;*.mkv"),
						new CommonFileDialogFilter(@"所有文件", @"*.*")
				}
			};
			if (dlg.ShowDialog(this) == CommonFileDialogResult.Ok)
			{
				InputFileTextBox2.Text = dlg.FileName;
			}
		}

		private void OutputButton2_OnClick(object sender, RoutedEventArgs e)
		{
			var dlg = new CommonSaveFileDialog
			{
				Title = @"另存为",
				AddToMostRecentlyUsedList = false,
				NavigateToShortcut = true,
				DefaultFileName = Path.GetFileName(OutputFileTextBox2.Text),
				DefaultExtension = Path.GetExtension(OutputFileTextBox2.Text),
				DefaultDirectory = Path.GetDirectoryName(OutputFileTextBox2.Text),
				Filters =
				{
						new CommonFileDialogFilter(@"视频文件", @"*.mp4;*.flv;*.mkv"),
						new CommonFileDialogFilter(@"所有文件", @"*.*")
				}
			};
			if (dlg.ShowDialog(this) == CommonFileDialogResult.Ok)
			{
				OutputFileTextBox2.Text = dlg.FileName;
			}
		}

		private void ConvertButton_OnClick(object sender, RoutedEventArgs e)
		{
			var inputPath = InputFileTextBox2.Text;
			var outputPath = OutputFileTextBox2.Text;
			var isDelete = IsDeleteCheckBox.IsChecked.GetValueOrDefault();
			var isDeleteToRecycle = IsDeleteToRecycleCheckBox.IsChecked.GetValueOrDefault();
			var fixTimestamp = FixTimestampCheckBox.IsChecked.GetValueOrDefault();

			if (File.Exists(inputPath))
			{
				MainWindowViewModel.AddConvertTask(inputPath, outputPath, isDelete, isDeleteToRecycle, fixTimestamp);
			}
			else if (Directory.Exists(inputPath))
			{
				if (!Directory.Exists(outputPath))
				{
					outputPath = inputPath;
				}
				var flvs = Utils.Utils.GetAllFlvList(inputPath);

				foreach (var flv in flvs)
				{
					MainWindowViewModel.AddConvertTask(flv, Path.Combine(outputPath, Path.ChangeExtension(flv, @"mp4")), isDelete, isDeleteToRecycle, fixTimestamp);
				}
			}

			InputFileTextBox2.Clear();
			OutputFileTextBox2.Clear();
		}

		private void RemoveTaskMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedItems = new List<VideoConvert>();
			foreach (var selectedItem in FlvDataGrid.SelectedItems)
			{
				if (selectedItem is VideoConvert vc)
				{
					selectedItems.Add(vc);
				}
			}

			foreach (var selectedItem in selectedItems)
			{
				selectedItem.Stop();
				MainWindowViewModel.VideoConverter.Remove(selectedItem);
			}
		}

		private void ClearTaskMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			foreach (var videoConvert in MainWindowViewModel.VideoConverter)
			{
				videoConvert.Stop();
			}
			MainWindowViewModel.VideoConverter.Clear();
		}

		#endregion

		#region 登录

		private void LoginButton_OnClick(object sender, RoutedEventArgs e)
		{
			LoginButton.IsEnabled = false;
			MainWindowViewModel.Login().ContinueWith(task =>
			{
				Dispatcher?.InvokeAsync(() =>
				{
					LoginButton.IsEnabled = true;
				});
			});
		}

		private void ApplyToApiButton_OnClick(object sender, RoutedEventArgs e)
		{
			ApplyToApiButton.IsEnabled = false;
			MainWindowViewModel.ApplyToApi().ContinueWith(task =>
			{
				Dispatcher?.InvokeAsync(() =>
				{
					ApplyToApiButton.IsEnabled = true;
				});
			});
		}

		#endregion
	}
}
