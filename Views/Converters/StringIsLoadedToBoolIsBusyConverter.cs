using System;
using System.Globalization;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class StringIsLoadedToBoolIsBusyConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringIsLoaded)
                return stringIsLoaded.Equals(ConstantStringKeys.NotLoadedState) ? true : stringIsLoaded.Equals(ConstantStringKeys.LoadedState) ? false : null;

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBusy)
                return isBusy ? ConstantStringKeys.NotLoadedState : ConstantStringKeys.LoadedState;

            return null;
        }
    }
}