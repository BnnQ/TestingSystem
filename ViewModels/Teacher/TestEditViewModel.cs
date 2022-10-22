using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Meziantou.Framework.WPF.Builders;
using Meziantou.Framework.WPF.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModels.Enums;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;
using Z.Linq;

namespace TestingSystem.ViewModels.Teacher
{
    public class TestEditViewModel : ValidatableViewModelBase
    {
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



        public Category[] Categories { get; private set; } = null!;
        public Models.Teacher[] Teachers { get; private set; } = null!;

        
        private readonly TestingSystemTeacherContext context;
        private readonly bool doesTestExistInDatabase;

        private readonly BackgroundWorker<Question> questionUpdaterFromDatabaseBackgroundWorker = new();
        
        public TestEditViewModel(Test test)
        {
            context = new TestingSystemTeacherContext();

            context.Categories.Include(category => category.Tests).Load();
            Categories = context.Categories.ToArray();

            context.Teachers.Include(teacher => teacher.OwnedTests).Load();
            Teachers = context.Teachers.ToArray();

            Test? testEntity = context.Tests.Find(test.Id);
            if (testEntity is not null)
            {
                doesTestExistInDatabase = true;
                EntityEntry<Test> testEntry = context.Entry(testEntity);
                testEntry.Reference(test => test.Category).Load();

                testEntry.Collection(test => test.Questions).Load();
                foreach (Question question in testEntity.Questions)
                    context.Entry(question).Collection(question => question.AnswerOptions).Load();

                testEntry.Collection(test => test.OwnerTeachers).Load();

                Test = testEntity;
            }
            else
            {
                doesTestExistInDatabase = false;
                Test = new(new ConcurrentObservableCollectionBuilder<Models.Teacher>(Teachers.Where(teacher => test.OwnerTeachers.Any(t => t.Id == teacher.Id))).Build());
            }

            SetupBackgroundWorkers();
            SetupValidator();
        }

        private void SetupBackgroundWorkers()
        {
            questionUpdaterFromDatabaseBackgroundWorker.OnWorkStarting = () => Mouse.OverrideCursor = Cursors.Wait;
            questionUpdaterFromDatabaseBackgroundWorker.DoWork = async (questions) =>
            {
                if (questions is not null && questions.Any())
                    await UpdateQuestionFromDatabaseAsync(questions.First());
            };
            questionUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () => Mouse.OverrideCursor = Cursors.Arrow;
        }

        private async Task UpdateQuestionFromDatabaseAsync(Question questionToBeUpdated)
        {
            using (TestingSystemTeacherContext questionLoaderContext = new())
            {
                Question? updatedQuestionEntity = await questionLoaderContext.FindAsync<Question>(questionToBeUpdated.Id);
                if (updatedQuestionEntity is not null)
                {
                    await questionLoaderContext
                        .Entry(updatedQuestionEntity)
                        .Collection(question => question.AnswerOptions)
                        .LoadAsync();

                    questionToBeUpdated.AnswerOptions = updatedQuestionEntity.AnswerOptions;
                    questionToBeUpdated.SerialNumberInTest = updatedQuestionEntity.SerialNumberInTest;
                    questionToBeUpdated.Content = updatedQuestionEntity.Content;
                    questionToBeUpdated.PointsCost = updatedQuestionEntity.PointsCost;
                    questionToBeUpdated.NumberOfSecondsToAnswer = updatedQuestionEntity.NumberOfSecondsToAnswer;
                }
                else
                {
                    OccurErrorMessage("Во время редактирования вопроса он был параллельно удалён другим пользователем или системой.");
                    Close(false);
                }
            }
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
            builder.RuleFor(viewModel => viewModel.MaximumPoints)
                .Must(maximumPoints => maximumPoints == Questions.Sum(question => question.PointsCost))
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    QuestionEditView editView = new(questionToBeAdded);
                    editViewDialogResult = editView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    Test.Questions.Add(questionToBeAdded);
            });
        }

        private AsyncRelayCommand<Question> editQuestionCommand = null!;
        public AsyncRelayCommand<Question> EditQuestionCommand
        {
            get => editQuestionCommand ??= new(async (question) =>
            {
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    QuestionEditView editView = new(question!);
                    editViewDialogResult = editView.ShowDialog();
                });

                if (doesTestExistInDatabase && editViewDialogResult == true && !questionUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await questionUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync(question!);
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

                if (!doesTestExistInDatabase)
                {
                    Category? categoryEntity = await context.FindAsync<Category>(Test.Category?.Id);
                    if (categoryEntity is not null)
                        categoryEntity.Tests.Add(Test);
                    else
                        OccurErrorMessage("Не удалось сохранить тест, так как во время его редактирования, содержащий тест категория была параллельно удалена другим пользователем или системой.");
                }

                await context.SaveChangesAsync();
                Close(true);
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
            Dispose(true);
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