using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoSplitVideo.View.ValueConverter
{
	public class LiveStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool isLive)
			{
				if (targetType == typeof(string))
				{
					return isLive ? @"正在直播" : @"未直播";
				}

				if (targetType == typeof(Brush))
				{
					return isLive ? Brushes.Green : Brushes.Red;
				}
			}
			return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
