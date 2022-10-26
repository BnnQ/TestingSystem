using System;
using System.Globalization;
using System.Windows.Data;

namespace TestingSystem.Views.Converters
{
    public class BoolByDoesQuestionOnlyHaveOneCorrectAnswerToStringConverter : IValueConverter
    {
        private const string onlyHaveOneCorrectAnswer = "Выберите один вариант ответа";
        private const string haveMultipleCorrectAnswers = "Выберите один или несколько вариантов ответа";

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool doesQuestionOnlyHaveOneCorrectAnswer)
                return doesQuestionOnlyHaveOneCorrectAnswer ? onlyHaveOneCorrectAnswer : haveMultipleCorrectAnswers;
            
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string doesQuestionOnlyHaveOneCorrectAnswerString)
                return value.Equals(onlyHaveOneCorrectAnswer) ? true : value.Equals(haveMultipleCorrectAnswers) ? false : null;

            return null;
        }
    }
}