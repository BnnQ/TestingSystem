using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ICollection<Question> questions = null!;
        public virtual ICollection<Question> Questions
        {
            get => questions;
            set
            {
                if (questions != value)
                {
                    if (value is ObservableCollection<Question>)
                        questions = value;
                    else
                        questions = new ObservableCollection<Question>(value);

                    OnPropertyChanged(nameof(Questions));
                    OnPropertyChanged(nameof(NumberOfQuestions));
                }
            }
        }

        private ushort questionsSeed = 0;
        public ushort NumberOfQuestions
        {
            get => (ushort) Questions.Count;
            set
            {
                if (Questions.Count == value)
                    return;

                if (Questions.Count < value)
                {
                    while (Questions.Count < value)
                        Questions.Add(new Question(this, ++questionsSeed));
                }
                else if (Questions.Count > value)
                {
                    Questions = new ObservableCollection<Question>(Questions.Take(value));
                }

                OnPropertyChanged(nameof(Questions));
            }
        }

        private ushort maximumPoints;
        public ushort MaximumPoints
        {
            get => maximumPoints;
            set
            {
                if (maximumPoints != value)
                {
                    maximumPoints = value;
                    OnPropertyChanged(nameof(MaximumPoints));
                }
            }
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
                    if (value is ObservableCollection<Teacher>)
                        ownerTeachers = value;
                    else
                        ownerTeachers = new ObservableCollection<Teacher>(value);

                    OnPropertyChanged(nameof(OwnerTeachers));
                }
            }
        }

        
        public Test()
        {
            Questions = new ObservableCollection<Question>();
            OwnerTeachers = new ObservableCollection<Teacher>();
        }
        public Test(ICollection<Teacher> ownerTeachers) : this()
        {
            Questions = new ObservableCollection<Question>();
            OwnerTeachers = ownerTeachers;
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

    }
}