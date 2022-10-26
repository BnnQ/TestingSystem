using System.Collections.Generic;
using TestingSystem.Models;

namespace TestingSystem.Helpers.Comparers
{
    public class AnswerOptionBySerialNumberComparer : IComparer<AnswerOption>
    {
        public int Compare(AnswerOption? x, AnswerOption? y)
        {
            if (x is null || y is null)
                return 0;

            return x.SerialNumberInQuestion - y.SerialNumberInQuestion;
        }
    }
}