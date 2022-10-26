using System;
using System.Globalization;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    /// <summary>
    /// One-way converter, that converts any <see cref="object"/> to <see cref="bool"/> based on whether it has a value (is not null)
    /// </summary>
    /// <returns><see langword="false"/> if value is not null, <see langword="true"/> if value is null</returns>
    public class ObjectToBoolByHasNotValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}