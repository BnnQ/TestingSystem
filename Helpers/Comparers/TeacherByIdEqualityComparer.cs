using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TestingSystem.Helpers.Comparers
{
    public class TeacherByIdEqualityComparer : IEqualityComparer<Models.Teacher>
    {
        public bool Equals(Models.Teacher? x, Models.Teacher? y)
        {
            if (x is null || y is null)
                return false;

            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode([DisallowNull] Models.Teacher obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}