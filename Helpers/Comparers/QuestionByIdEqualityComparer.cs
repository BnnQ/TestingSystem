using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TestingSystem.Models;

namespace TestingSystem.Helpers.Comparers
{
    public class QuestionByIdEqualityComparer : IEqualityComparer<Question>
    {
        public bool Equals(Question? x, Question? y)
        {
            if (x is null || y is null)
                return false;

            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode([DisallowNull] Question obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}