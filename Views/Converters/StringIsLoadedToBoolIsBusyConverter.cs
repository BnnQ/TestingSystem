using System;
using System.Globalization;
using System.Windows.Data;
using TestingSystem.Constants;

namespace TestingSystem.Views.Converters
{
    public class StringIsLoadedToBoolIsBusyConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringIsLoaded)
                return stringIsLoaded.Equals(LoadStates.NotLoaded) ? true : stringIsLoaded.Equals(LoadStates.Loaded) ? false : null;

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBusy)
                return isBusy ? LoadStates.NotLoaded : LoadStates.Loaded;

            return null;
        }
    }
}