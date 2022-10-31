
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TestingSystem.Helpers.Comparers{
    public class StudentByIdEqualityComparer : IEqualityComparer<Models.Student>
    {
        public bool Equals(Models.Student? x, Models.Student? y)
        {
            if (x is null || y is null)
                return false;

            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode([DisallowNull] Models.Student obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}