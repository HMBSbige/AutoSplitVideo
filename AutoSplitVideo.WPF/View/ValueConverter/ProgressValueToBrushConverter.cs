using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoSplitVideo.View.ValueConverter
{
	public sealed class ProgressValueToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double p && p > 90)
			{
				return new SolidColorBrush(Colors.Red);
			}
			return new SolidColorBrush(Color.FromRgb(38, 160, 218));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
