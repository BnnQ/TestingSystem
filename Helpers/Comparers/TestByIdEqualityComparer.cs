using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TestingSystem.Models;

namespace TestingSystem.Helpers.Comparers
{
    public class TestByIdEqualityComparer : IEqualityComparer<Test>
    {
        public bool Equals(Test? x, Test? y)
        {
            if (x is null || y is null)
                return false;

            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode([DisallowNull] Test obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
