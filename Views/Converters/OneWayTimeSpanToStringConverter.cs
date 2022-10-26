using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class OneWayTimeSpanToStringConverter : IValueConverter
    {
        private const string NullValue = "0 сек.";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                string longTimeString = new DateTime(timeSpan.Ticks).ToLongTimeString();
                string[] splittedLongTime = longTimeString.Split(':');

                byte.TryParse(splittedLongTime.First(), out byte hours);
                byte.TryParse(splittedLongTime[1], out byte minutes);
                byte.TryParse(splittedLongTime.Last(), out byte seconds);

                if (hours == 0 && minutes == 0 && seconds == 0)
                    return NullValue;

                StringBuilder resultStringBuilder = new();
                if (hours > 0)
                    resultStringBuilder.Append($"{hours} час.");
                if (minutes > 0)
                    resultStringBuilder.Append($"{minutes} мин.");
                if (seconds > 0)
                    resultStringBuilder.Append($"{seconds} сек.");

                return resultStringBuilder.ToString();
            }

            return NullValue;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}