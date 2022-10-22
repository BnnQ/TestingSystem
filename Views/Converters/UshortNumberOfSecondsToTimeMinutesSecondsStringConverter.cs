using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class UshortNumberOfSecondsToTimeMinutesSecondsStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ushort numberOfSeconds)
            {
                string timeMinutesSecondsString = new DateTime(TimeSpan.TicksPerSecond * numberOfSeconds).ToLongTimeString();
                return timeMinutesSecondsString.Remove(0, timeMinutesSecondsString.IndexOf(':') + 1);
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string timeMinutesSeconds && !string.IsNullOrWhiteSpace(timeMinutesSeconds))
            {
                string[] splittedShortTimeString = timeMinutesSeconds.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (splittedShortTimeString.Length != 2)
                    return null;

                byte.TryParse(splittedShortTimeString.First(), out byte minutes);
                if (minutes > 59)
                    minutes = 59;

                byte.TryParse(splittedShortTimeString.Last(), out byte seconds);
                if (seconds > 59)
                    seconds = 59;

                const byte SecondsInMinute = 60;

                ushort numberOfSeconds = (ushort) ((minutes * SecondsInMinute) + seconds);
                return numberOfSeconds;
            }

            return null;
        }

    }
}