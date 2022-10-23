using CommunityToolkit.Mvvm.ComponentModel;
using Meziantou.Framework.WPF.Collections;
using Meziantou.Framework.WPF.Builders;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;

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

        private ushort answerOptionsSeed = 0;
        private void UpdateAnswerOptionsSeed()
        {
            if (AnswerOptions?.Count == 0)
            {
                answerOptionsSeed = 0;
            }
            else
            {
                ushort maximumSerialNumberInQuestionOfAnswerOptions = AnswerOptions!
                                                                      .MaxBy(answerOption => answerOption.SerialNumberInQuestion)!
                                                                      .SerialNumberInQuestion;
                answerOptionsSeed = maximumSerialNumberInQuestionOfAnswerOptions;
            }
        }
        private bool isAutoAnswerOptionNumberingEnabled;
        public bool IsAutoAnswerOptionNumberingEnabled
        {
            get => isAutoAnswerOptionNumberingEnabled;
            set
            {
                if (isAutoAnswerOptionNumberingEnabled != value)
                {
                    isAutoAnswerOptionNumberingEnabled = value;
                    OnPropertyChanged(nameof(IsAutoAnswerOptionNumberingEnabled));

                    if (IsAutoAnswerOptionNumberingEnabled)
                        RenumberAnswerOptions();
                }
            }
        }
        private void RenumberAnswerOptions()
        {
            answerOptionsSeed = 0;
            foreach (AnswerOption answerOption in AnswerOptions)
                answerOption.SerialNumberInQuestion = ++answerOptionsSeed;
        }

        private void OnAnswerOptionsItemPropertyChanged(object? _, PropertyChangedEventArgs? __)
        {
            if (IsAutoAnswerOptionNumberingEnabled)
                RenumberAnswerOptions();
            else
                UpdateAnswerOptionsSeed();
        }
        private void OnAnswerOptionsChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove)
            {
                OnPropertyChanged(nameof(NumberOfAnswerOptions));

                if (IsAutoAnswerOptionNumberingEnabled)
                    RenumberAnswerOptions();
                else
                    UpdateAnswerOptionsSeed();
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
                    answerOptions = new ConcurrentObservableCollectionBuilder<AnswerOption>(value)
                                    .WhichToHandleCollectionChangesUses(OnAnswerOptionsChanged)
                                    .WhichToHandleItemsPropertyChangedUses(OnAnswerOptionsItemPropertyChanged)
                                    .Build();

                    OnPropertyChanged(nameof(AnswerOptions));
                    OnPropertyChanged(nameof(NumberOfAnswerOptions));
                }
            }
        }

        public ushort NumberOfAnswerOptions
        {
            get => (ushort) AnswerOptions.Count;
            set
            {
                if (AnswerOptions.Count == value || value > 1000)
                    return;

                if (AnswerOptions.Count < value)
                {
                    while (AnswerOptions.Count < value)
                        AnswerOptions.Add(new AnswerOption(this, ++answerOptionsSeed));
                }
                else if (AnswerOptions.Count > value)
                {
                    AnswerOptions = new ConcurrentObservableCollectionBuilder<AnswerOption>(AnswerOptions.Take(value)).Build();
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

        public int TestId { get; set; }
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
            AnswerOptions = new ConcurrentObservableCollection<AnswerOption>();
        }
        public Question(Test test, ushort serialNumberInTest) : this()
        {
            Test = test;
            SerialNumberInTest = serialNumberInTest;
            Content = $"{SerialNumberInTest} вопрос";
            PointsCost = 1;
            NumberOfAnswerOptions = 1;
        }
        public Question(Test test, ushort serialNumberInTest, double pointsCost) : this(test, serialNumberInTest)
        {
            PointsCost = pointsCost;
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
            ushort? numberOfSecondsToAnswer, Test test, ushort serialNumberInTest) 
            : this(content, pointsCost, answerOptions, test, serialNumberInTest)
        {
            NumberOfSecondsToAnswer = numberOfSecondsToAnswer;
        }
        public Question(string content, double pointsCost, ushort numberOfAnswerOptions, ushort? numberOfSecondsToAnswer, Test test,
            ushort serialNumberInTest) : this(content, pointsCost, numberOfAnswerOptions, test, serialNumberInTest)
        {
            NumberOfSecondsToAnswer = numberOfSecondsToAnswer;
        }

        public ushort GetSerialNumberForNewAnswerOption() => ++answerOptionsSeed;

    }
}