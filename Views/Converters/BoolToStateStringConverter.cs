using System;
using System.Globalization;
using System.Windows.Data;
using TestingSystem.Constants;

namespace TestingSystem.Views.Converters
{
    public class BoolToStateStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? TurningStates.Enabled : TurningStates.Disabled;
            else
                return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringState)
                return stringState.Equals(TurningStates.Enabled) ? true : stringState.Equals(TurningStates.Disabled) ? false : null;

            return null;
        }

    }
}