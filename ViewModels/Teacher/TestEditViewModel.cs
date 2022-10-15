using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModelsLibrary.Enumerables;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;
using Z.Linq;

namespace TestingSystem.ViewModels.Teacher
{
    public class TestEditViewModel : ValidatableViewModelBase
    {
        private Test test = null!;
        public Category[] Categories { get; private set; } = null!;
        public Models.Teacher[] Teachers { get; private set; } = null!;

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
                    
                    if (value.HasValue)
                        NumberOfSecondsToComplete = null;

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

                    if (value.HasValue)
                        NumberOfSecondsToAnswerEachQuestion = null;

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

        private ushort NumberOfOwnerTeachers => (ushort) ownerTeachers.Count;

        private readonly bool doesTestExistInDatabase;
        private readonly BackgroundWorker initialDatabaseLoaderBackgroundWorker = new();

        public TestEditViewModel(Test test)
        {
            SetupBackgroundWorkers();
            _ = initialDatabaseLoaderBackgroundWorker.RunWorkerAsync();

            TestingSystemTeacherContext context = new();
            Test? testEntity = context.Tests.Find(test.Id);
            if (testEntity is not null)
            {
                doesTestExistInDatabase = true;

                EntityEntry<Test> testEntry = context.Entry(testEntity);
                testEntry.Collection(test => test.Questions).Load();
                testEntry.Collection(test => test.OwnerTeachers).Load();

                this.test = testEntity;
                context.Dispose();
            }
            else
            {
                context.Dispose();

                doesTestExistInDatabase = false;
                this.test = test;
            }

            Name = this.test.Name;
            Questions = new ObservableCollection<Question>(this.test.Questions.OrderBy(question => question.SerialNumberInTest));
            Questions.CollectionChanged += (_, eventArgs) =>
            {
                if (eventArgs.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    Questions = new(Questions.OrderBy(question => question.SerialNumberInTest));
                    if (NumberOfSecondsToAnswerEachQuestion.HasValue)
                        ApplyNumberOfSecondsToAnswerEachQuestion();
                }
            };

            NumberOfQuestions = this.test.NumberOfQuestions;
            Category = this.test.Category;
            MaximumPoints = this.test.MaximumPoints;
            NumberOfSecondsToAnswerEachQuestion = this.test.NumberOfSecondsToAnswerEachQuestion;
            NumberOfSecondsToComplete = this.test.NumberOfSecondsToComplete;
            IsAccountingForIncompleteAnswersEnabled = this.test.IsAccountingForIncompleteAnswersEnabled;
            OwnerTeachers = new ObservableCollection<Models.Teacher>(this.test.OwnerTeachers);

            SetupValidator();
        }


        private void SetupBackgroundWorkers()
        {
            initialDatabaseLoaderBackgroundWorker.DoWork = async () =>
            {
                using (TestingSystemTeacherContext context = new())
                {
                    await context.Categories.LoadAsync();
                    Categories = await context.Categories.Local.ToArrayAsync();

                    await context.Teachers.Include(teacher => teacher.OwnedTests).LoadAsync();
                    Teachers = await context.Teachers.Local.ToArrayAsync();
                }
            };
        }

        #region Validation setup
        private ValidationState nameValidationState = ValidationState.Disabled;
        private ValidationState maximumPointsValidationState = ValidationState.Disabled;
        private ValidationState numberOfQuestionsValidationState = ValidationState.Disabled;
        private ValidationState numberOfOwnerTeachersValidationState = ValidationState.Disabled;
        private ValidationState categoryValidationState = ValidationState.Disabled;

        protected override void SetupValidator()
        {
            ValidationBuilder<TestEditViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Name)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .When(viewModel => viewModel.nameValidationState == ValidationState.Enabled)
                .WithMessage("Название теста не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Name)
                .MaxLength(255)
                .When(viewModel => viewModel.nameValidationState == ValidationState.Enabled)
                .WithMessage("Название теста не может быть длиннее 255 символов");

            builder.RuleFor(viewModel => viewModel.MaximumPoints)
                .Must(maximumPoints => maximumPoints > 0)
                .When(viewModel => viewModel.maximumPointsValidationState == ValidationState.Enabled)
                .WithMessage("Максимальное количество баллов не может быть меньше 1");

            builder.RuleFor(viewModel => viewModel.NumberOfQuestions)
                .Must(numberOfQuestions => numberOfQuestions > 0)
                .When(viewModel => viewModel.numberOfQuestionsValidationState == ValidationState.Enabled)
                .WithMessage("В тесте должен присутствовать как минимум 1 вопрос");

            builder.RuleFor(viewModel => viewModel.NumberOfOwnerTeachers)
                .Must(numberOfOwnerTeachers => numberOfOwnerTeachers > 0)
                .When(viewModel => viewModel.numberOfOwnerTeachersValidationState == ValidationState.Enabled)
                .WithMessage("У теста должен быть как минимум 1 владелец");

            builder.RuleFor(viewModel => viewModel.Category)
                .NotNull()
                .When(viewModel => viewModel.categoryValidationState == ValidationState.Enabled)
                .WithMessage("Тест обязательно должен находиться в категории");

            Validator = builder.Build(this);
        }

        private async Task<bool> IsNameValidAsync()
        {
            nameValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            nameValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsMaximumPointsValidAsync()
        {
            maximumPointsValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            maximumPointsValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsNumberOfQuestionsValidAsync()
        {
            numberOfQuestionsValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            numberOfQuestionsValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsNumberOfOwnerTeachersValidAsync()
        {
            numberOfOwnerTeachersValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            numberOfQuestionsValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsCategoryValidAsync()
        {
            categoryValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            categoryValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }
        #endregion

        private void ApplyNumberOfSecondsToAnswerEachQuestion()
        {
            foreach (Question question in Questions)
                question.NumberOfSecondsToAnswer = NumberOfSecondsToAnswerEachQuestion;
        }

        #region Commands
        private bool AreOwnerTeachersMoreThanOne() => OwnerTeachers.Count > 1;
        private bool IsTeacherOwnerOfTest(Models.Teacher teacher) => OwnerTeachers.Contains(teacher);

        private RelayCommand addQuestionCommand = null!;
        public RelayCommand AddQuestionCommand
        {
            get => addQuestionCommand ??= new(() =>
            {
                Question questionToBeAdded = new(test, ++questionsSeed);

                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    QuestionEditView editView = new(questionToBeAdded);
                    editViewDialogResult = editView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    Questions.Add(questionToBeAdded);
            });
        }

        private RelayCommand<Question> editQuestionCommand = null!;
        public RelayCommand<Question> EditQuestionCommand
        {
            get => editQuestionCommand ??= new((question) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    QuestionEditView editView = new(question!);
                    editView.ShowDialog();
                });
            }, (question) => question is not null);
        }

        private RelayCommand<Question> removeQuestionCommand = null!;
        public RelayCommand<Question> RemoveQuestionCommand
        {
            get => removeQuestionCommand ??= new(
                (question) => Questions.Remove(question!),
                (question) => question is not null);
        }

        private RelayCommand<Models.Teacher> addTestOwnerCommand = null!;
        public RelayCommand<Models.Teacher> AddTestOwnerCommand
        {
            get => addTestOwnerCommand ??= new((teacherToBeAdded) =>
            {
                if (!teacherToBeAdded!.OwnedTests.Contains(test))
                    OwnerTeachers.Add(teacherToBeAdded);
            }, 
            (teacherToBeAdded) => teacherToBeAdded is not null && !IsTeacherOwnerOfTest(teacherToBeAdded));
        }

        private RelayCommand<Models.Teacher> removeTestOwnerCommand = null!;
        public RelayCommand<Models.Teacher> RemoveTestOwnerCommand
        {
            get => removeTestOwnerCommand ??= new(
                (testOwner) => OwnerTeachers.Remove(testOwner!),
                (testOwner) => testOwner is not null && AreOwnerTeachersMoreThanOne());
        }

        private void SaveTestChangesLocally()
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
        }
        private AsyncRelayCommand confirmCommand = null!;
        public AsyncRelayCommand ConfirmCommand
        {
            get => confirmCommand ??= new(async () =>
            {
                if (!await IsNameValidAsync() || !await IsCategoryValidAsync() || !await IsNumberOfQuestionsValidAsync() ||
                    !await IsMaximumPointsValidAsync() || !await IsNumberOfOwnerTeachersValidAsync())
                {
                    return;
                }


                if (doesTestExistInDatabase)
                {
                    using (TestingSystemTeacherContext context = new())
                    {
                        test = (await context.FindAsync<Test>(test.Id))!;
                        SaveTestChangesLocally();
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    SaveTestChangesLocally();
                }
                

                Close(true);
            }, () => !initialDatabaseLoaderBackgroundWorker.IsBusy);
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion
    }
}