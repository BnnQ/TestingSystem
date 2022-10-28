using System;

namespace TestingSystem.Models
{
    public class TestResult
    {
        public int Id { get; set; }

        public int TestId { get; set; }
        public virtual Test Test { get; set; } = null!;

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public ushort Score { get; set; }
        public ushort NumberOfCorrectAnswers { get; set; }
        public ushort NumberOfIncorrectAnswers { get; set; }
        public TimeSpan TestCompletionTime { get; set; }
        public TimeSpan AverageAnswerTime { get; set; }
        public DateTime CompletionDate { get; set; }


        public TestResult(ushort score, ushort numberOfCorrectAnswers, ushort numberOfIncorrectAnswers,
            TimeSpan testCompletionTime, TimeSpan averageAnswerTime, DateTime completionDate)
        {
            Score = score;
            NumberOfCorrectAnswers = numberOfCorrectAnswers;
            NumberOfIncorrectAnswers = numberOfIncorrectAnswers;
            TestCompletionTime = testCompletionTime;
            AverageAnswerTime = averageAnswerTime;
            CompletionDate = completionDate;
        }
        public TestResult(Test test, Student student, ushort score, ushort numberOfCorrectAnswers, ushort numberOfIncorrectAnswers,
            TimeSpan testCompletionTime, TimeSpan averageAnswerTime, DateTime completionDate)
            : this(score, numberOfIncorrectAnswers, numberOfIncorrectAnswers, testCompletionTime, averageAnswerTime, completionDate)
        {
            Test = test;
            Student = student;
            Score = score;
            NumberOfCorrectAnswers = numberOfCorrectAnswers;
            NumberOfIncorrectAnswers = numberOfIncorrectAnswers;
            TestCompletionTime = testCompletionTime;
            AverageAnswerTime = averageAnswerTime;
            CompletionDate = completionDate;
        }

    }
}