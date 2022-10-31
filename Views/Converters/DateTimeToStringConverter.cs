using System;
using System.Globalization;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
                return dateTime.ToString(CultureInfo.CurrentCulture);

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateTimeString && DateTime.TryParse(dateTimeString, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateTime))
                return dateTime;

            return null;
        }
    }
}