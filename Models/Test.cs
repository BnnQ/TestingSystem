using CommunityToolkit.Mvvm.ComponentModel;
using Meziantou.Framework.WPF.Builders;
using Meziantou.Framework.WPF.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace TestingSystem.Models
{
    public class Test : ObservableObject
    {
        public int Id { get; set; }

        private string name = null!;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private ushort questionsSeed = 0;
        private void UpdateQuestionsSeed()
        {
            if (Questions?.Count == 0)
            {
                questionsSeed = 0;
            }
            else
            {
                ushort maximumSerialNumberInTestOfQuestions = Questions!
                                                             .MaxBy(question => question.SerialNumberInTest)!
                                                             .SerialNumberInTest;
                questionsSeed = maximumSerialNumberInTestOfQuestions;
            }
        }

        private bool isAutoQuestionNumberingEnabled;
        public bool IsAutoQuestionNumberingEnabled
        {
            get => isAutoQuestionNumberingEnabled;
            set
            {
                if (isAutoQuestionNumberingEnabled != value)
                {
                    isAutoQuestionNumberingEnabled = value;
                    OnPropertyChanged(nameof(IsAutoQuestionNumberingEnabled));

                    if (IsAutoQuestionNumberingEnabled)
                        RenumberQuestions();
                }
            }
        }
        private void RenumberQuestions()
        {
            questionsSeed = 0;
            foreach (Question question in Questions)
                question.SerialNumberInTest = ++questionsSeed;
        }

        private bool isAutoCalculationOfQuestionsCostEnabled;
        public bool IsAutoCalculationOfQuestionsCostEnabled
        {
            get => isAutoCalculationOfQuestionsCostEnabled;
            set
            {
                if (isAutoCalculationOfQuestionsCostEnabled != value)
                {
                    isAutoCalculationOfQuestionsCostEnabled = value;
                    OnPropertyChanged(nameof(IsAutoCalculationOfQuestionsCostEnabled));

                    if (IsAutoCalculationOfQuestionsCostEnabled)
                        CalculateCostOfQuestions();
                }
            }
        }
        private void CalculateCostOfQuestions()
        {
            double costPerQuestion = (double) MaximumPoints / NumberOfQuestions;
            foreach (Question question in Questions)
                question.PointsCost = costPerQuestion;
        }

        private void OnQuestionsItemPropertyChanged(object? _, PropertyChangedEventArgs? __)
        {
            if (IsAutoQuestionNumberingEnabled)
                RenumberQuestions();
            else
                UpdateQuestionsSeed();

            if (IsAutoCalculationOfQuestionsCostEnabled)
                CalculateCostOfQuestions();
        }
        private void OnQuestionsChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove)
            {
                OnPropertyChanged(nameof(NumberOfQuestions));

                if (IsAutoQuestionNumberingEnabled)
                    RenumberQuestions();
                else
                    UpdateQuestionsSeed();

                if (IsAutoCalculationOfQuestionsCostEnabled)
                    CalculateCostOfQuestions();
            }

            if (NumberOfSecondsToAnswerEachQuestion.HasValue && args.NewItems?.Count > 0)
            {
                foreach (Question question in args.NewItems)
                {
                    if (!question.NumberOfSecondsToAnswer.HasValue)
                        question.NumberOfSecondsToAnswer = NumberOfSecondsToAnswerEachQuestion;
                }
            }

        }
        private ICollection<Question> questions = null!;
        public virtual ICollection<Question> Questions
        {
            get => questions;
            set
            {
                if (questions != value)
                {
                    questions = new ConcurrentObservableCollectionBuilder<Question>(value)
                                .WhichToHandleCollectionChangesUses(OnQuestionsChanged)
                                .Build();

                    OnPropertyChanged(nameof(Questions));
                    OnPropertyChanged(nameof(NumberOfQuestions));
                }
            }
        }

        public ushort NumberOfQuestions
        {
            get => (ushort) Questions.Count;
            set
            {
                if (Questions.Count == value || value > 1000)
                    return;

                if (Questions.Count < value)
                {
                    while (Questions.Count < value)
                        Questions.Add(new Question(this, ++questionsSeed));
                }
                else if (Questions.Count > value)
                {
                    Questions = new ConcurrentObservableCollectionBuilder<Question>(Questions.Take(value))
                                .Build();
                }

                OnPropertyChanged(nameof(Questions));
                OnPropertyChanged(nameof(NumberOfQuestions));
            }
        }

        private ushort maximumPoints = 1;
        public ushort MaximumPoints
        {
            get => maximumPoints;
            set
            {
                if (maximumPoints != value && value > 0)
                {
                    maximumPoints = value;
                    OnPropertyChanged(nameof(MaximumPoints));

                    if (IsAutoCalculationOfQuestionsCostEnabled)
                        CalculateCostOfQuestions();
                }
            }
        }

        private void ApplyNumberOfSecondsToAnswerEachQuestion()
        {
            foreach (Question question in Questions)
                question.NumberOfSecondsToAnswer = NumberOfSecondsToAnswerEachQuestion;
        }
        private ushort? numberOfSecondsToAnswerEachQuestion;
        public ushort? NumberOfSecondsToAnswerEachQuestion
        {
            get => numberOfSecondsToAnswerEachQuestion;
            set
            {
                if (numberOfSecondsToAnswerEachQuestion != value)
                {
                    numberOfSecondsToAnswerEachQuestion = value;
                    OnPropertyChanged(nameof(NumberOfSecondsToAnswerEachQuestion));

                    if (NumberOfSecondsToAnswerEachQuestion.HasValue)
                    {
                        if (NumberOfSecondsToComplete.HasValue)
                            NumberOfSecondsToComplete = null;
                    }

                    ApplyNumberOfSecondsToAnswerEachQuestion();
                }
            }
        }

        private ushort? numberOfSecondsToComplete;
        public ushort? NumberOfSecondsToComplete
        {
            get => numberOfSecondsToComplete;
            set
            {
                if (numberOfSecondsToComplete != value)
                {
                    numberOfSecondsToComplete = value;
                    OnPropertyChanged(nameof(NumberOfSecondsToComplete));

                    if (NumberOfSecondsToComplete.HasValue)
                    {
                        if (NumberOfSecondsToAnswerEachQuestion.HasValue)
                            NumberOfSecondsToAnswerEachQuestion = null;
                    }
                }
            }
        }

        private bool isAccountingForIncompleteAnswersEnabled;
        public bool IsAccountingForIncompleteAnswersEnabled
        {
            get => isAccountingForIncompleteAnswersEnabled;
            set
            {
                if (isAccountingForIncompleteAnswersEnabled != value)
                {
                    isAccountingForIncompleteAnswersEnabled = value;
                    OnPropertyChanged(nameof(IsAccountingForIncompleteAnswersEnabled));
                }
            }
        }

        public int CategoryId { get; set; }
        private Category category = null!;
        public virtual Category Category
        {
            get => category;
            set
            {
                if (category != value)
                {
                    category = value;
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        private ICollection<Teacher> ownerTeachers = null!;
        public virtual ICollection<Teacher> OwnerTeachers
        {
            get => ownerTeachers;
            set
            {
                if (ownerTeachers != value)
                {
                    ownerTeachers = new ConcurrentObservableCollectionBuilder<Teacher>(value).Build();
                    OnPropertyChanged(nameof(OwnerTeachers));
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
                    if (value is ImmutableHashSet<TestResult> immutableTestResults)
                        testResults = immutableTestResults;
                    else
                        testResults = value.ToImmutableHashSet();
                }
            }
        }


        public Test()
        {
            Questions = new ConcurrentObservableCollection<Question>();
            OwnerTeachers = new ConcurrentObservableCollection<Teacher>();
            TestResults = new HashSet<TestResult>();
        }
        public Test(ICollection<Teacher> ownerTeachers, ushort numberOfQuestions = 1) : this()
        {
            OwnerTeachers = ownerTeachers;
            NumberOfQuestions = numberOfQuestions;
        }
        public Test(string name, ICollection<Question> questions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers) : this()
        {
            Name = name;
            Questions = questions;
            MaximumPoints = maximumPoints;
            IsAccountingForIncompleteAnswersEnabled = isAccountingForIncompleteAnswersEnabled;
            Category = category;
            OwnerTeachers = ownerTeachers;
        }
        public Test(string name, ushort numberOfQuestions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers) : this()
        {
            Name = name;
            NumberOfQuestions = numberOfQuestions;
            MaximumPoints = maximumPoints;
            IsAccountingForIncompleteAnswersEnabled = isAccountingForIncompleteAnswersEnabled;
            Category = category;
            OwnerTeachers = ownerTeachers;
        }
        public Test(string name, ICollection<Question> questions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers, ICollection<TestResult> testResults) : this()
        {
            Name = name;
            Questions = questions;
            MaximumPoints = maximumPoints;
            IsAccountingForIncompleteAnswersEnabled = isAccountingForIncompleteAnswersEnabled;
            Category = category;
            OwnerTeachers = ownerTeachers;
            TestResults = testResults;
        }
        public Test(string name, ushort numberOfQuestions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers, ICollection<TestResult> testResults) : this()
        {
            Name = name;
            NumberOfQuestions = numberOfQuestions;
            MaximumPoints = maximumPoints;
            IsAccountingForIncompleteAnswersEnabled = isAccountingForIncompleteAnswersEnabled;
            Category = category;
            OwnerTeachers = ownerTeachers;
            TestResults = testResults;
        }
        public Test(string name, ICollection<Question> questions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers,
            ushort? numberOfSecondsToAnswerEachQuestion, ushort? numberOfSecondsToComplete)
            : this(name, questions, maximumPoints, isAccountingForIncompleteAnswersEnabled, category, ownerTeachers)
        {
            if (numberOfSecondsToAnswerEachQuestion.HasValue)
                NumberOfSecondsToAnswerEachQuestion = numberOfSecondsToAnswerEachQuestion;
            else
                NumberOfSecondsToComplete = numberOfSecondsToComplete;
        }
        public Test(string name, ushort numberOfQuestions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers,
            ushort? numberOfSecondsToAnswerEachQuestion, ushort? numberOfSecondsToComplete)
            : this(name, numberOfQuestions, maximumPoints, isAccountingForIncompleteAnswersEnabled, category, ownerTeachers)
        {
            if (numberOfSecondsToAnswerEachQuestion.HasValue)
                NumberOfSecondsToAnswerEachQuestion = numberOfSecondsToAnswerEachQuestion;
            else
                NumberOfSecondsToComplete = numberOfSecondsToComplete;
        }
        public Test(string name, ICollection<Question> questions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers, ICollection<TestResult> testResults,
            ushort? numberOfSecondsToAnswerEachQuestion, ushort? numberOfSecondsToComplete)
            : this(name, questions, maximumPoints, isAccountingForIncompleteAnswersEnabled, category, ownerTeachers, testResults)
        {
            if (numberOfSecondsToAnswerEachQuestion.HasValue)
                NumberOfSecondsToAnswerEachQuestion = numberOfSecondsToAnswerEachQuestion;
            else
                NumberOfSecondsToComplete = numberOfSecondsToComplete;
        }
        public Test(string name, ushort numberOfQuestions, ushort maximumPoints, bool isAccountingForIncompleteAnswersEnabled,
            Category category, ICollection<Teacher> ownerTeachers, ICollection<TestResult> testResults,
            ushort? numberOfSecondsToAnswerEachQuestion, ushort? numberOfSecondsToComplete)
            : this(name, numberOfQuestions, maximumPoints, isAccountingForIncompleteAnswersEnabled, category, ownerTeachers, testResults)
        {
            if (numberOfSecondsToAnswerEachQuestion.HasValue)
                NumberOfSecondsToAnswerEachQuestion = numberOfSecondsToAnswerEachQuestion;
            else
                NumberOfSecondsToComplete = numberOfSecondsToComplete;
        }

        public ushort GetSerialNumberForNewQuestion() => ++questionsSeed;

    }
}