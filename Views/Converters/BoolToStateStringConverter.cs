using System;
using System.Globalization;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class BoolToStateStringConverter : IValueConverter
    {
        private const string EnabledState = "Включено";
        private const string DisabledState = "Выключено";

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? EnabledState : DisabledState;
            else
                return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringState)
                return stringState.Equals(EnabledState) ? true : stringState.Equals(DisabledState) ? false : null;

            return null;
        }

    }
}