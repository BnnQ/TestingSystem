using System;
using System.Globalization;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class NullableDateTimeToNullableLongTimeStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? dateTime = value as DateTime?;
            if (dateTime.HasValue)
                return dateTime.Value.ToLongTimeString();

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string longTimeString && !string.IsNullOrWhiteSpace(longTimeString))
                return DateTime.Parse(longTimeString);

            return null;
        }
    }
}