using System.Collections.Generic;
using TestingSystem.Models;

namespace TestingSystem.Helpers.Comparers
{
    public class QuestionBySerialNumberComparer : IComparer<Question>
    {
        public int Compare(Question? x, Question? y)
        {
            if (x is null || y is null)
                return 0;
            else
                return x.SerialNumberInTest - y.SerialNumberInTest;
        }
    }
}
