using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TestingSystem.Views.Converters
{
    public class OneWayNullableDateTimeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? dateTime = value as DateTime?;
            if (dateTime.HasValue)
            {
                long numberOfSeconds = dateTime.Value.Ticks / TimeSpan.TicksPerSecond;
                if (numberOfSeconds <= 10)
                    return Brushes.Red;
                else if (numberOfSeconds <= 60)
                    return Brushes.Orange;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}