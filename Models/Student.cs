using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using TestingSystem.Helpers.Comparers;

namespace TestingSystem.Models
{
    [ObservableObject]
    public partial class Student : User
    {
        private readonly static IEqualityComparer<TestResult> testResultComparer = new TestResultByIdEqualityComparer();

        public int Id { get; set; }

        private string fullName = null!;
        public string FullName
        {
            get => fullName;
            set
            {
                if (fullName != value)
                {
                    fullName = value;
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private ICollection<TestResult> testResults = null!;
        public virtual ICollection<TestResult> TestResults
        {
            get => testResults;
            set
            {
                if (testResults != value)
                {
                    if (value is HashSet<TestResult> hashSetTestResults)
                        testResults = hashSetTestResults;
                    else
                        testResults = value.ToHashSet(testResultComparer);
                }
            }
        }


        public Student(string name, string hashedPassword) : base(name, hashedPassword)
        {
            TestResults = new HashSet<TestResult>(testResultComparer);
        }
        public Student(string name, string hashedPassword, string fullName) : this(name, hashedPassword)
        {
            FullName = fullName;
        }
        public Student(string name, string hashedPassword, string fullName, ICollection<TestResult> testResults)
            : this(name, hashedPassword, fullName)
        {
            TestResults = testResults;
        }


    }
}