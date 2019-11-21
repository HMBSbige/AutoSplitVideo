using System;
using System.Windows;

namespace AutoSplitVideo.View.Control
{
	public partial class TimeControl
	{
		public TimeControl()
		{
			InitializeComponent();
		}

		#region Text

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty =
				DependencyProperty.Register("Text", typeof(string), typeof(TimeControl),
						new UIPropertyMetadata(string.Empty, OnTimeChanged));


		private static void OnTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			if (!(obj is TimeControl control)) return;
			var str = (string)e.NewValue;
			if (str.Length < 12)
			{
				control.Text = @"00:00:00.000";
				return;
			}
			if (!int.TryParse(str.Substring(0, 2), out var hours))
			{
				hours = 0;
			}

			if (!int.TryParse(str.Substring(3, 2), out var minutes))
			{
				minutes = 0;
			}

			if (!int.TryParse(str.Substring(6, 2), out var seconds))
			{
				seconds = 0;
			}

			if (!int.TryParse(str.Substring(9, 3), out var milliseconds))
			{
				milliseconds = 0;
			}

			var time = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds) + TimeSpan.FromMilliseconds(milliseconds);

			control.Text = $@"{EnsureHours(time.TotalHours)}:{time.Minutes.ToString().PadLeft(2, '0')}:{time.Seconds.ToString().PadLeft(2, '0')}.{time.Milliseconds.ToString().PadLeft(3, '0')}";
		}

		private static string EnsureHours(double hours)
		{
			if (hours < 0)
			{
				hours = 0;
			}
			else if (hours > 99)
			{
				hours = 99;
			}
			return ((int)hours).ToString().PadLeft(2, '0');
		}

		#endregion
	}
}
