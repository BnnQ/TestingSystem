using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Meziantou.Framework.WPF.Builders;
using Microsoft.EntityFrameworkCore;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModels.Enums;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TestingSystem.Helpers.Comparers;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class TestEditViewModel : ValidatableViewModelBase
    {
        private readonly static IEqualityComparer<Category> categoryEqualityComparer = new CategoryByIdEqualityComparer();
        private readonly static IEqualityComparer<Models.Teacher> teacherEqualityComparer = new TeacherByIdEqualityComparer();

        public void OnTestChanged(object? _, System.ComponentModel.PropertyChangedEventArgs args) => OnPropertyChanged(args.PropertyName);
        private Test test = null!;
        public Test Test
        {
            get => test;
            set
            {
                if (test != value)
                {
                    if (test is not null)
                        test.PropertyChanged -= OnTestChanged;

                    test = value;
                    if (test is not null)
                        test.PropertyChanged += OnTestChanged;

                    OnPropertyChanged(nameof(Test));
                }
            }
        }

        public string Name
        {
            get => Test.Name;
            set => Test.Name = value;
        }

        public ICollection<Question> Questions
        {
            get => Test.Questions;
            set => Test.Questions = value;
        }

        public bool IsAutoCalculationOfQuestionsCostEnabled
        {
            get => Test.IsAutoCalculationOfQuestionsCostEnabled;
            set => Test.IsAutoCalculationOfQuestionsCostEnabled = value;
        }

        public bool IsAutoQuestionNumberingEnabled
        {
            get => Test.IsAutoQuestionNumberingEnabled;
            set => Test.IsAutoQuestionNumberingEnabled = value;
        }

        public ushort NumberOfQuestions
        {
            get => Test.NumberOfQuestions;
            set => Test.NumberOfQuestions = value;
        }

        public ushort MaximumPoints
        {
            get => Test.MaximumPoints;
            set => Test.MaximumPoints = value;
        }

        public ushort? NumberOfSecondsToAnswerEachQuestion
        {
            get => Test.NumberOfSecondsToAnswerEachQuestion;
            set => Test.NumberOfSecondsToAnswerEachQuestion = value;
        }

        public ushort? NumberOfSecondsToComplete
        {
            get => Test.NumberOfSecondsToComplete;
            set => Test.NumberOfSecondsToComplete = value;
        }

        public bool IsAccountingForIncompleteAnswersEnabled
        {
            get => Test.IsAccountingForIncompleteAnswersEnabled;
            set => Test.IsAccountingForIncompleteAnswersEnabled = value;
        }
        public Category Category
        {
            get => Test.Category;
            set => Test.Category = value;
        }

        private void OnOwnerTeachersChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove)
                OnPropertyChanged(nameof(NumberOfOwnerTeachers));
        }
        public ICollection<Models.Teacher> OwnerTeachers
        {
            get => Test.OwnerTeachers;
            set
            {
                Test.OwnerTeachers = new ConcurrentObservableCollectionBuilder<Models.Teacher>(value)
                                     .WhichToHandleCollectionChangesUses(OnOwnerTeachersChanged)
                                     .Build();
            }
        }
        public int NumberOfOwnerTeachers => OwnerTeachers.Count;


        private ImmutableHashSet<Category> categories = null!;
        public ImmutableHashSet<Category> Categories
        {
            get => categories;
            set
            {
                if (value != categories)
                {
                    if (value.KeyComparer != categoryEqualityComparer)
                        value = value.WithComparer(categoryEqualityComparer);

                    categories = value;
                }
            }
        }
        private ImmutableHashSet<Models.Teacher> teachers = null!;
        public ImmutableHashSet<Models.Teacher> Teachers 
        {
            get => teachers;
            set
            {
                if (teachers != value)
                {
                    if (value.KeyComparer != teacherEqualityComparer)
                        value = value.WithComparer(teacherEqualityComparer);

                    teachers = value;
                }
            }
        }

        
        private readonly TestingSystemTeacherContext context = null!;
        private bool doesTestExistInDatabase;
        public BackgroundWorker<Test> InitialLoaderBackgroundWorker { get; init; } = new();
        public BackgroundWorker ConfirmerBackgroundWorker { get; init; } = new();
        
        public TestEditViewModel(Test test)
        {
            context = new TestingSystemTeacherContext();

            SetupBackgroundWorkers();
            _ = InitialLoaderBackgroundWorker.RunWorkerAsync(test);
        }

        private void SetupBackgroundWorkers()
        {
            InitialLoaderBackgroundWorker.DoWork = async (parameters) =>
            {
                if (parameters?.Length < 1)
                    return;

                Test test = parameters!.First();
                try
                {
                    await context.Categories
                                 .Include(category => category.Tests)  
                                 .LoadAsync();
                    Categories = await Task.Run(() => context.Categories.ToImmutableHashSet(categoryEqualityComparer));

                    await context.Teachers
                                 .Include(teacher => teacher.OwnedTests)
                                 .LoadAsync();
                    Teachers = await Task.Run(() => context.Teachers.ToImmutableHashSet(teacherEqualityComparer));

                    await context.Tests
                                 .Include(t => t.Category)
                                 .Include(t => t.Questions)
                                      .ThenInclude(q => q.AnswerOptions)
                                 .Include(t => t.OwnerTeachers)
                                 .Where(t => t.Id == test.Id)
                                 .LoadAsync();

                    Test? testEntity = await context.Tests.FindAsync(test.Id);
                    if (testEntity is not null)
                    {
                        doesTestExistInDatabase = true;
                        Test = testEntity;
                    }
                    else
                    {
                        doesTestExistInDatabase = false;
                        Test = new(new ConcurrentObservableCollectionBuilder<Models.Teacher>(Teachers.Where(teacher => test.OwnerTeachers.Any(t => t.Id == teacher.Id))).Build());
                    }
                }
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }

                SetupValidator();
            };

            ConfirmerBackgroundWorker.DoWork = async () =>
            {
                if (!await IsNameValidAsync() || !await IsCategoryValidAsync() || !await IsNumberOfQuestionsValidAsync() ||
                    !await IsMaximumPointsValidAsync() || !await IsNumberOfOwnerTeachersValidAsync() || !await IsIsAutoQuestionNumberingEnabledValidAsync())
                {
                    return;
                }

                try
                {
                    if (!doesTestExistInDatabase)
                    {
                        Category? categoryEntity = await context.FindAsync<Category>(Test.Category?.Id);
                        if (categoryEntity is not null)
                            categoryEntity.Tests.Add(Test);
                        else
                            throw new NullReferenceException("Не удалось сохранить тест, так как во время его редактирования, содержащий тест категория была параллельно удалена другим пользователем или системой.");
                    }

                    await context.SaveChangesAsync();
                    Close(true);
                }
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }
            };
        }

        #region Validation setup
        private ValidationState nameValidationState = ValidationState.Disabled;
        private ValidationState maximumPointsValidationState = ValidationState.Disabled;
        private ValidationState numberOfQuestionsValidationState = ValidationState.Disabled;
        private ValidationState numberOfOwnerTeachersValidationState = ValidationState.Disabled;
        private ValidationState categoryValidationState = ValidationState.Disabled;
        private ValidationState isAutoQuestionNumberingEnabledValidationState = ValidationState.Disabled;

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
            builder.RuleFor(viewModel => viewModel.MaximumPoints)
                .Must(maximumPoints => maximumPoints == Math.Round(Questions.Sum(question => question.PointsCost)))
                .When(viewModel => viewModel.maximumPointsValidationState == ValidationState.Enabled)
                .WithMessage("Сумма стоимости вопросов должна быть равна максимальному количеству баллов за тест");

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

            builder.RuleFor(viewModel => viewModel.IsAutoQuestionNumberingEnabled)
                .Must(_ => (_ || !_) && Questions.DistinctBy(question => question.SerialNumberInTest).Count() == Questions.Count)
                .When(viewModel => viewModel.isAutoQuestionNumberingEnabledValidationState == ValidationState.Enabled)
                .WithMessage("Порядковые номера вопросов не могут повторяться");

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

        private async Task<bool> IsIsAutoQuestionNumberingEnabledValidAsync()
        {
            isAutoQuestionNumberingEnabledValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            isAutoQuestionNumberingEnabledValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }
        #endregion

        #region Commands
        private bool AreOwnerTeachersMoreThanOne() => Test.OwnerTeachers.Count > 1;
        private bool IsTeacherOwnerOfTest(Models.Teacher teacher) => Test.OwnerTeachers.FirstOrDefault(t => t.Id == teacher.Id) is not null;

        private RelayCommand addQuestionCommand = null!;
        public RelayCommand AddQuestionCommand
        {
            get => addQuestionCommand ??= new(() =>
            {
                Question questionToBeAdded = new(Test, Test.GetSerialNumberForNewQuestion());

                bool? editViewDialogResult = default;
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    QuestionEditView editView = new(questionToBeAdded);
                    editViewDialogResult = editView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    Test.Questions.Add(questionToBeAdded);
            });
        }

        private RelayCommand<Question> editQuestionCommand = null!;
        public RelayCommand<Question> EditQuestionCommand
        {
            get => editQuestionCommand ??= new((question) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
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
                (question) => Test.Questions.Remove(question!),
                (question) => question is not null);
        }

        private RelayCommand<Models.Teacher> addTestOwnerCommand = null!;
        public RelayCommand<Models.Teacher> AddTestOwnerCommand
        {
            get => addTestOwnerCommand ??= new((teacherToBeAdded) =>
            {
                if (!teacherToBeAdded!.OwnedTests.Contains(Test))
                    Test.OwnerTeachers.Add(teacherToBeAdded);
            }, 
            (teacherToBeAdded) => teacherToBeAdded is not null && !IsTeacherOwnerOfTest(teacherToBeAdded));
        }

        private RelayCommand<Models.Teacher> removeTestOwnerCommand = null!;
        public RelayCommand<Models.Teacher> RemoveTestOwnerCommand
        {
            get => removeTestOwnerCommand ??= new(
                (testOwner) => Test.OwnerTeachers.Remove(testOwner!),
                (testOwner) => testOwner is not null && AreOwnerTeachersMoreThanOne());
        }

        private AsyncRelayCommand confirmAsyncCommand = null!;
        public AsyncRelayCommand ConfirmAsyncCommand
        {
            get => confirmAsyncCommand ??= new(async () =>
            {
                if (!ConfirmerBackgroundWorker.IsBusy)
                    await ConfirmerBackgroundWorker.RunWorkerAsync();
            });
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion

        #region Disposing
        public override void Close(bool? dialogResult = null)
        {
            Dispose();
            base.Close(dialogResult);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        private bool isDisposed = false;
        protected override void Dispose(bool needDisposing)
        {
            if (isDisposed)
                return;

            if (needDisposing)
            {
                context.Dispose();
            }

            isDisposed = true;
        }

        ~TestEditViewModel() => Dispose(false);
        #endregion
    }
}