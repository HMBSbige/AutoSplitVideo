using AutoSplitVideo.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoSplitVideo.View.ValueConverter
{
	public class RecordingStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is RecordingStatus status)
			{
				if (targetType == typeof(string))
				{
					return status switch
					{
						RecordingStatus.Stopped => @"未在录制",
						RecordingStatus.Starting => @"启动录制中",
						RecordingStatus.Recording => @"录制中",
						_ => @"未知"
					};
				}

				if (targetType == typeof(Brush))
				{
					return status switch
					{
						RecordingStatus.Stopped => Brushes.Red,
						RecordingStatus.Starting => Brushes.Yellow,
						RecordingStatus.Recording => Brushes.Green,
						_ => Brushes.Red
					};
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
