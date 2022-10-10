using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestingSystem.Models
{
    public class Question : ObservableObject
    {
        public int Id { get; set; }

        private string content = null!;
        public string Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        private double pointsCost;
        public double PointsCost
        {
            get => pointsCost;
            set
            {
                if (pointsCost != value)
                {
                    pointsCost = value;
                    OnPropertyChanged(nameof(PointsCost));
                }
            }
        }

        private ICollection<AnswerOption> answerOptions = null!;
        public virtual ICollection<AnswerOption> AnswerOptions
        {
            get => answerOptions;
            set
            {
                if (answerOptions != value)
                {
                    if (value is ObservableCollection<AnswerOption>)
                        answerOptions = value;
                    else
                        answerOptions = new ObservableCollection<AnswerOption>(value);

                    OnPropertyChanged(nameof(AnswerOptions));
                    OnPropertyChanged(nameof(NumberOfAnswerOptions));
                }
            }
        }

        private ushort answerOptionsSeed = 0;
        public ushort NumberOfAnswerOptions
        {
            get => (ushort) AnswerOptions.Count;
            set
            {
                if (AnswerOptions.Count == value)
                    return;

                if (AnswerOptions.Count > value)
                {
                    AnswerOptions = new ObservableCollection<AnswerOption>(AnswerOptions.Take(value));
                }
                else if (AnswerOptions.Count < value)
                {
                    while (AnswerOptions.Count < value)
                        AnswerOptions.Add(new AnswerOption(this, ++answerOptionsSeed));
                }

                OnPropertyChanged(nameof(AnswerOptions));
            }
        }

        private ushort? numberOfSecondsToAnswer;
        public ushort? NumberOfSecondsToAnswer
        {
            get => numberOfSecondsToAnswer;
            set
            {
                if (numberOfSecondsToAnswer != value)
                {
                    numberOfSecondsToAnswer = value;
                    OnPropertyChanged(nameof(NumberOfSecondsToAnswer));
                }
            }
        }

        private int testId;
        public int TestId
        {
            get => testId;
            set
            {
                testId = value;

                if (Test is not null && Test.Id != testId)
                    Test.Id = testId;
            }
        }

        private Test test = null!;
        public virtual Test Test
        {
            get => test;
            set
            {
                if (test != value)
                {
                    test = value;
                    OnPropertyChanged(nameof(Test));
                }
            }
        }

        private ushort serialNumberInTest;
        public ushort SerialNumberInTest
        {
            get => serialNumberInTest;
            set
            {
                if (serialNumberInTest != value)
                {
                    serialNumberInTest = value;
                    OnPropertyChanged(nameof(SerialNumberInTest));
                }
            }
        }



        public Question()
        {
            AnswerOptions = new ObservableCollection<AnswerOption>();
        }
        public Question(Test test, ushort serialNumberInTest) : this()
        {
            Test = test;
            SerialNumberInTest = serialNumberInTest;
        }
        public Question(string content, double pointsCost, ICollection<AnswerOption> answerOptions,
            Test test, ushort serialNumberInTest)
        {
            Content = content;
            PointsCost = pointsCost;
            AnswerOptions = answerOptions;
            Test = test;
            SerialNumberInTest = serialNumberInTest;
        }
        public Question(string content, double pointsCost, ushort numberOfAnswerOptions, Test test, ushort serialNumberInTest) : this()
        {
            Content = content;
            PointsCost = pointsCost;
            NumberOfAnswerOptions = numberOfAnswerOptions;
            Test = test;
            SerialNumberInTest = serialNumberInTest;
        }
        public Question(string content, double pointsCost, ICollection<AnswerOption> answerOptions,
            ushort numberOfSecondsToAnswer, Test test, ushort serialNumberInTest) 
            : this(content, pointsCost, answerOptions, test, serialNumberInTest)
        {
            NumberOfSecondsToAnswer = numberOfSecondsToAnswer;
        }
        public Question(string content, double pointsCost, ushort numberOfAnswerOptions, ushort numberOfSecondsToAnswer, Test test,
            ushort serialNumberInTest) : this(content, pointsCost, numberOfAnswerOptions, test, serialNumberInTest)
        {
            NumberOfSecondsToAnswer = numberOfSecondsToAnswer;
        }

    }
}