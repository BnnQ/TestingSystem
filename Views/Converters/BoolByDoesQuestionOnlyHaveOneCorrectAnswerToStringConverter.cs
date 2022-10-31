using System;
using System.Globalization;
using System.Windows.Data;
using TestingSystem.Constants;

namespace TestingSystem.Views.Converters
{
    public class BoolByDoesQuestionOnlyHaveOneCorrectAnswerToStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool doesQuestionOnlyHaveOneCorrectAnswer)
                return doesQuestionOnlyHaveOneCorrectAnswer ? NumberOfCorrectAnswers.OneCorrectAnswer : NumberOfCorrectAnswers.MultipleCorrectAnswers;
            
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string doesQuestionOnlyHaveOneCorrectAnswerString)
                return doesQuestionOnlyHaveOneCorrectAnswerString.Equals(NumberOfCorrectAnswers.OneCorrectAnswer) ? true : doesQuestionOnlyHaveOneCorrectAnswerString.Equals(NumberOfCorrectAnswers.MultipleCorrectAnswers) ? false : null;

            return null;
        }
    }
}