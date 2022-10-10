using System;

namespace TestingSystem.Models
{
    public record TestResults
    {
        public Test Test { get; init; }
        public ushort Score { get; init; }
        public ushort NumberOfCorrectAnswers { get; init; }
        public ushort NumberOfIncorrectAnswers { get; init; }
        public TimeSpan TestCompletionTime { get; init; }
        public TimeSpan AverageAnswerTime { get; init; }

        public TestResults(Test test, ushort score, ushort numberOfCorrectAnswers, ushort numberOfIncorrectAnswers,
            TimeSpan testCompletionTime, TimeSpan averageAnswerTime)
        {
            Test = test;
            Score = score;
            NumberOfCorrectAnswers = numberOfCorrectAnswers;
            NumberOfIncorrectAnswers = numberOfIncorrectAnswers;
            TestCompletionTime = testCompletionTime;
            AverageAnswerTime = averageAnswerTime;
        }

    }
}