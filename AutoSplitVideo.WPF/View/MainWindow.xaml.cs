using AutoSplitVideo.ViewModel;
using System.Windows;

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

		private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
