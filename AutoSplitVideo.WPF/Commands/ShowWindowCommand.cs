#pragma warning disable CS0067

using AutoSplitVideo.ViewModel;
using System;
using System.Windows.Input;

namespace AutoSplitVideo.Commands
{
	public class ShowWindowCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			if (parameter is MainWindowViewModel viewModel)
			{
				viewModel.TriggerShowHide();
			}
		}

		public event EventHandler CanExecuteChanged;
	}
}
