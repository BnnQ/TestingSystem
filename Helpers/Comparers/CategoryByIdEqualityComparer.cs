using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TestingSystem.Models;

namespace TestingSystem.Helpers.Comparers
{
    public class CategoryByIdEqualityComparer : IEqualityComparer<Category>
    {
        public bool Equals(Category? x, Category? y)
        {
            if (x is null || y is null)
                return false;

            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode([DisallowNull] Category obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}