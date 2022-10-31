using System;
using System.Globalization;
using System.Windows.Data;
using TestingSystem.Constants.Teacher;

namespace TestingSystem.Views.Converters
{
    public class BoolByIsTeacherOwnerToStringAccessConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTeacherOwner)
                return isTeacherOwner ? AccessModes.ReadAndWrite : AccessModes.ReadOnly;

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string accessMode)
                return accessMode.Equals(AccessModes.ReadAndWrite) ? true : accessMode.Equals(AccessModes.ReadOnly) ? AccessModes.ReadOnly : null;

            return null;
        }

    }
}