using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common;
using NeoSmart.AsyncLock;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class TestEditViewModel : ViewModelBase
    {
        private readonly Test test;
        public Category[] Categories { get; init; }

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

        private ObservableCollection<Question> questions = null!;
        public ObservableCollection<Question> Questions
        {
            get => questions;
            set
            {
                if (questions != value)
                {
                    questions = value;
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
                        Questions.Add(new Question(test, ++questionsSeed));
                }
                else if (Questions.Count > value)
                {
                    Questions = new ObservableCollection<Question>(Questions.Take(value));
                }

                OnPropertyChanged(nameof(Questions));
            }
        }

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
                numberOfSecondsToComplete = value;
                OnPropertyChanged(nameof(NumberOfSecondsToComplete));
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

        private ObservableCollection<Models.Teacher> ownerTeachers = null!;
        public virtual ObservableCollection<Models.Teacher> OwnerTeachers
        {
            get => ownerTeachers;
            set
            {
                if (ownerTeachers != value)
                {
                    ownerTeachers = value;
                    OnPropertyChanged(nameof(OwnerTeachers));
                }
            }
        }

        private readonly TestingSystemTeacherContext databaseContext;
        private readonly AsyncLock databaseContextLocker;

        public TestEditViewModel(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker,
            Category[] categories, Test test)
        {
            this.databaseContext = databaseContext;
            this.databaseContextLocker = databaseContextLocker;
            this.test = test;
            Categories = categories;

            Name = test.Name;
            Questions = new ObservableCollection<Question>(test.Questions);
            NumberOfQuestions = test.NumberOfQuestions;
            Category = test.Category;
            MaximumPoints = test.MaximumPoints;
            NumberOfSecondsToAnswerEachQuestion = test.NumberOfSecondsToAnswerEachQuestion;
            NumberOfSecondsToComplete = test.NumberOfSecondsToComplete;
            IsAccountingForIncompleteAnswersEnabled = test.IsAccountingForIncompleteAnswersEnabled;
            OwnerTeachers = new ObservableCollection<Models.Teacher>(test.OwnerTeachers);
        }


        #region Commands
        private AsyncRelayCommand<Question> editQuestionAsyncCommand = null!;
        public AsyncRelayCommand<Question> EditQuestionAsyncCommand
        {
            get => editQuestionAsyncCommand ??= new(async (question) =>
            {
                Question? questionFromDatabase = default;
                using (await databaseContextLocker.LockAsync())
                {
                    questionFromDatabase = await databaseContext.FindAsync<Question>(question!.Id);
                    if (questionFromDatabase is not null)
                    {
                        await databaseContext.Entry(questionFromDatabase)
                        .Collection(question => question.AnswerOptions)
                        .LoadAsync();
                    }
                }

                if (questionFromDatabase is not null)
                {
                    bool? editViewDialogResult = default;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        QuestionEditView editView = new(databaseContext, databaseContextLocker, questionFromDatabase);
                        editViewDialogResult = editView.ShowDialog();
                    });

                    if (editViewDialogResult == true)
                    {
                        using (await databaseContextLocker.LockAsync())
                            await databaseContext.SaveChangesAsync();
                    }
                }
            }, (question) => question is not null);
        }

        private RelayCommand confirmCommand = null!;
        public RelayCommand ConfirmCommand
        {
            get => confirmCommand ??= new(() =>
            {
                test.Name = Name;
                test.Questions = Questions;
                test.NumberOfQuestions = NumberOfQuestions;
                test.Category = Category;
                test.MaximumPoints = MaximumPoints;
                test.NumberOfSecondsToAnswerEachQuestion = NumberOfSecondsToAnswerEachQuestion;
                test.NumberOfSecondsToComplete = NumberOfSecondsToComplete;
                test.IsAccountingForIncompleteAnswersEnabled = IsAccountingForIncompleteAnswersEnabled;
                test.OwnerTeachers = OwnerTeachers;

                Close(true);
            },
           () => !string.IsNullOrWhiteSpace(Name) && NumberOfQuestions > 0 && Category is not null &&
           MaximumPoints > 0 && OwnerTeachers.Count > 0);
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion

    }
}