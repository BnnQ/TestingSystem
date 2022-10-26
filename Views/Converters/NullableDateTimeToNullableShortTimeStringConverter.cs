using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class NullableDateTimeToNullableShortTimeStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? dateTime = value as DateTime?;
            if (dateTime.HasValue)
            {
                string[] splittedTime = dateTime.Value.ToLongTimeString().Split(':', StringSplitOptions.RemoveEmptyEntries);
                return $"{splittedTime[1]}:{splittedTime.Last()}";
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string shortTimeString && !string.IsNullOrWhiteSpace(shortTimeString))
                return DateTime.Parse(shortTimeString);

            return null;
        }
    }
}