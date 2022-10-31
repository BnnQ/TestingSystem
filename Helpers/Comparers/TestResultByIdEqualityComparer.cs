using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TestingSystem.Models;

namespace TestingSystem.Helpers.Comparers
{
    public class TestResultByIdEqualityComparer : IEqualityComparer<TestResult>
    {
        public bool Equals(TestResult? x, TestResult? y)
        {
            if (x is null || y is null)
                return false;

            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode([DisallowNull] TestResult obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}