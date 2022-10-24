using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class UshortNumberOfSecondsToTimeHoursMinutesStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ushort numberOfSeconds)
            {
                return new DateTime(TimeSpan.TicksPerSecond * numberOfSeconds).ToShortTimeString();
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string timeHoursMinutesString && !string.IsNullOrWhiteSpace(timeHoursMinutesString))
            {
                string[] splittedShortTimeString = timeHoursMinutesString.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (splittedShortTimeString.Length != 2)
                    return null;

                byte.TryParse(splittedShortTimeString.First(), out byte hours);
                if (hours > 23)
                    hours = 23;

                byte.TryParse(splittedShortTimeString.Last(), out byte minutes);
                if (minutes > 59)
                    minutes = 59;

                const ushort SecondsInMinute = 60;
                const ushort MinutesInHour = 60;
                const ushort SecondsInHour = 1 * SecondsInMinute * MinutesInHour;

                ushort numberOfSeconds = (ushort) ((hours * SecondsInHour) + (minutes * SecondsInMinute));
                return numberOfSeconds;
            }

            return null;
        }

    }
}