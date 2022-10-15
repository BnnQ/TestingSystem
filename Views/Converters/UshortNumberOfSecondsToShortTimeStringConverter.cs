using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class UshortNumberOfSecondsToShortTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ushort numberOfSeconds)
            {
                return new DateTime(TimeSpan.TicksPerSecond * numberOfSeconds).ToShortTimeString();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string shortTimeString && !string.IsNullOrWhiteSpace(shortTimeString))
            {
                string[] splittedShortTimeString = shortTimeString.Split(':', StringSplitOptions.RemoveEmptyEntries);
                byte hours = byte.Parse(splittedShortTimeString.First());
                byte minutes = byte.Parse(splittedShortTimeString.Last());

                const ushort SecondsInMinute = 60;
                const ushort MinutesInHour = 60;
                const ushort SecondsInHour = 1 * SecondsInMinute * MinutesInHour;

                ushort numberOfSeconds = (ushort) ((hours * SecondsInHour) + (minutes * SecondsInMinute));
                return numberOfSeconds;
            }

            return DependencyProperty.UnsetValue;
        }

    }
}