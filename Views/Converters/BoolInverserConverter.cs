using System;
using System.Globalization;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class BoolInverserConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueToConvert)
                return !valueToConvert;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueToConvert)
                return !valueToConvert;

            return null;
        }
    }
}