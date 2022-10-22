using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModels.Enums;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class QuestionEditViewModel : ValidatableViewModelBase
    {
        private void OnQuestionChanged(object? _, System.ComponentModel.PropertyChangedEventArgs args) => OnPropertyChanged(args.PropertyName);
        private Question question = null!;
        public Question Question
        {
            get => question;
            set
            {
                if (question != value)
                {
                    if (question is not null)
                        question.PropertyChanged -= OnQuestionChanged;

                    question = value;
                    OnPropertyChanged(nameof(Question));

                    if (question is not null)
                        question.PropertyChanged += OnQuestionChanged;
                }
            }
        }

        public string Content
        {
            get => Question.Content;
            set => Question.Content = value;
        }

        public double PointsCost
        {
            get => Question.PointsCost;
            set => Question.PointsCost = value;
        }

        public ICollection<AnswerOption> AnswerOptions
        {
            get => Question.AnswerOptions;
            set => Question.AnswerOptions = value;
        }

        public bool IsAutoAnswerOptionNumberingEnabled
        {
            get => Question.IsAutoAnswerOptionNumberingEnabled;
            set => Question.IsAutoAnswerOptionNumberingEnabled = value;
        }

        public ushort NumberOfAnswerOptions
        {
            get => Question.NumberOfAnswerOptions;
            set => Question.NumberOfAnswerOptions = value;
        }

        public ushort? NumberOfSecondsToAnswer
        {
            get => Question.NumberOfSecondsToAnswer;
            set => Question.NumberOfSecondsToAnswer = value;
        }

        public ushort SerialNumberInTest
        {
            get => Question.SerialNumberInTest;
            set => Question.SerialNumberInTest = value;
        }


        private readonly TestingSystemTeacherContext context;
        private bool doesQuestionExistInDatabase;
        private readonly bool doesTestExistInDatabase;

        private readonly BackgroundWorker<AnswerOption> answerOptionUpdaterFromDatabaseBackgroundWorker = new();

        public QuestionEditViewModel(Question question)
        {
            context = new TestingSystemTeacherContext();

            Question? questionEntity = context.Find<Question>(question.Id);
            if (questionEntity is not null)
            {
                doesQuestionExistInDatabase = true;
                doesTestExistInDatabase = true;

                EntityEntry<Question> questionEntry = context.Entry(questionEntity);
                questionEntry.Collection(question => question.AnswerOptions).Load();

                Question = questionEntity;
            }
            else if (question.Test.Id == 0)
            {
                doesTestExistInDatabase = false;
                doesQuestionExistInDatabase = false;
                Question = question;
            }
            else
            {
                doesQuestionExistInDatabase = false;
                Question = new(question.Test, question.SerialNumberInTest, question.PointsCost);
            }

            SetupBackgroundWorkers();
            SetupValidator();
        }

        private void SetupBackgroundWorkers()
        {
            answerOptionUpdaterFromDatabaseBackgroundWorker.OnWorkStarting = () => Mouse.OverrideCursor = Cursors.Wait;
            answerOptionUpdaterFromDatabaseBackgroundWorker.DoWork = async (answerOptions) =>
            {
                if (answerOptions is not null && answerOptions.Any())
                    await UpdateAnswerOptionsFromDatabaseAsync(answerOptions.First());
            };
            answerOptionUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () => Mouse.OverrideCursor = Cursors.Arrow;
        }

        private async Task UpdateAnswerOptionsFromDatabaseAsync(AnswerOption answerOptionToBeUpdated)
        {
            using (TestingSystemTeacherContext answerOptionLoaderContext = new())
            {
                AnswerOption? updatedAnswerOptionEntity = await answerOptionLoaderContext.FindAsync<AnswerOption>(answerOptionToBeUpdated.Id);
                if (updatedAnswerOptionEntity is not null)
                {
                    answerOptionToBeUpdated.SerialNumberInQuestion = updatedAnswerOptionEntity.SerialNumberInQuestion;
                    answerOptionToBeUpdated.Content = updatedAnswerOptionEntity.Content;
                    answerOptionToBeUpdated.IsCorrect = updatedAnswerOptionEntity.IsCorrect;
                }
                else
                {
                    OccurErrorMessage("Во время редактирования варианта ответа он был параллельно удалён другим пользователем или системой.");
                    Close(false);
                }
            }
        }

        #region Validation setup
        private ValidationState contentValidationState = ValidationState.Disabled;
        private ValidationState pointsCostValidationState = ValidationState.Disabled;
        private ValidationState numberOfAnswerOptionsValidationState = ValidationState.Disabled;
        protected override void SetupValidator()
        {
            ValidationBuilder<QuestionEditViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Content)
                .Must(content => !string.IsNullOrWhiteSpace(content))
                .When(viewModel => viewModel.contentValidationState == ValidationState.Enabled)
                .WithMessage("Вопрос не может быть пустым");

            builder.RuleFor(viewModel => viewModel.PointsCost)
                .Must(pointsCost => pointsCost > 0)
                .When(viewModel => viewModel.pointsCostValidationState == ValidationState.Enabled)
                .WithMessage("Стоимость вопроса не моежт быть меньше или равна 0");

            builder.RuleFor(viewModel => viewModel.NumberOfAnswerOptions)
                .Must(numberOfAnswerOptions => numberOfAnswerOptions > 0)
                .When(viewModel => viewModel.numberOfAnswerOptionsValidationState == ValidationState.Enabled)
                .WithMessage("Должен быть как минимум один вариант ответа");

            Validator = builder.Build(this);
        }

        private async Task<bool> IsContentValidAsync()
        {
            contentValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            contentValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsPointsCostValidAsync()
        {
            pointsCostValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            pointsCostValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsNumberOfAnswerOptionsValidAsync()
        {
            numberOfAnswerOptionsValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            numberOfAnswerOptionsValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }
        #endregion

        #region Commands
        private RelayCommand addAnswerOptionCommand = null!;
        public RelayCommand AddAnswerOptionCommand
        {
            get => addAnswerOptionCommand ??= new(() =>
            {
                AnswerOption answerOptionToBeAdded = new(question, question.GetSerialNumberForNewAnswerOption());
                
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AnswerOptionEditView editView = new(answerOptionToBeAdded);
                    editViewDialogResult = editView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    AnswerOptions.Add(answerOptionToBeAdded);
            });
        }

        private RelayCommand<AnswerOption> editAnswerOptionCommand = null!;
        public RelayCommand<AnswerOption> EditAnswerOptionCommand
        {
            get => editAnswerOptionCommand ??= new((answerOption) =>
            {
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AnswerOptionEditView editView = new(answerOption!);
                    editViewDialogResult = editView.ShowDialog();
                });

                if (doesQuestionExistInDatabase && editViewDialogResult == true && !answerOptionUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    _ = answerOptionUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
            }, (answerOption) => answerOption is not null);
        }

        private RelayCommand<AnswerOption> removeAnswerOptionCommand = null!;
        public RelayCommand<AnswerOption> RemoveAnswerOptionCommand
        {
            get => removeAnswerOptionCommand ??= new(
                (answerOption) => AnswerOptions.Remove(answerOption!),
                (answerOption) => answerOption is not null);
        }

        private AsyncRelayCommand confirmAsyncCommand = null!;
        public AsyncRelayCommand ConfirmAsyncCommand
        {
            get => confirmAsyncCommand ??= new(async () =>
            {
                if (!await IsContentValidAsync() || !await IsPointsCostValidAsync() || !await IsNumberOfAnswerOptionsValidAsync())
                {
                    return;
                }

                if (!doesTestExistInDatabase)
                {
                    Close(true);
                    return;
                }

                if (!doesQuestionExistInDatabase)
                {
                    Test? testEntity = await context.FindAsync<Test>(Question.Test.Id);
                    if (testEntity is not null)
                        testEntity.Questions.Add(Question);
                    else
                        OccurErrorMessage("Не удалось сохранить вопрос, так как во время его редактирования, содержащий вопрос тест был параллельно удалён другим пользователем или системой.");
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

        ~QuestionEditViewModel() => Dispose(false);
        #endregion
    }
}