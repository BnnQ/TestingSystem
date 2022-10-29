using System.Collections.Generic;
using TestingSystem.Models;

namespace TestingSystem.Helpers.Comparers
{
    public class TestResultByCompletionDateComparer : IComparer<TestResult>
    {
        public int Compare(TestResult? x, TestResult? y)
        {
            if (x is null || y is null)
                return 0;

            long timeSpanTicks = (x.CompletionDate - y.CompletionDate).Ticks;
            return timeSpanTicks < 0 ? -1 : timeSpanTicks == 0 ? 0 : 1;
        }
    }
}