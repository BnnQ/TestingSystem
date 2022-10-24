using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModels.Enums;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class QuestionEditViewModel : ValidatableViewModelBase
    {
        private readonly Question questionBackup;

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

        public QuestionEditViewModel(Question question)
        {
            Question = question;
            questionBackup = new(question.Content, question.PointsCost, question.AnswerOptions,
                question.NumberOfSecondsToAnswer, question.Test, question.SerialNumberInTest);

            SetupValidator();
        }

        #region Validation setup
        private ValidationState contentValidationState = ValidationState.Disabled;
        private ValidationState pointsCostValidationState = ValidationState.Disabled;
        private ValidationState numberOfAnswerOptionsValidationState = ValidationState.Disabled;
        private ValidationState isAutoAnswerOptionNumberingEnabledValidationState = ValidationState.Disabled;
        protected override void SetupValidator()
        {
            ValidationBuilder<QuestionEditViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Content)
                .Must(content => !string.IsNullOrWhiteSpace(content))
                .When(viewModel => viewModel.contentValidationState == ValidationState.Enabled)
                .WithMessage("Вопрос не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Content)
                .MaxLength(512)
                .When(viewModel => viewModel.contentValidationState == ValidationState.Enabled)
                .WithMessage("Вопрос не может быть длиннее 512 символов");

            builder.RuleFor(viewModel => viewModel.PointsCost)
                .Must(pointsCost => pointsCost > 0)
                .When(viewModel => viewModel.pointsCostValidationState == ValidationState.Enabled)
                .WithMessage("Стоимость вопроса не моежт быть меньше или равна 0");

            builder.RuleFor(viewModel => viewModel.NumberOfAnswerOptions)
                .Must(numberOfAnswerOptions => numberOfAnswerOptions > 0)
                .When(viewModel => viewModel.numberOfAnswerOptionsValidationState == ValidationState.Enabled)
                .WithMessage("Должен быть как минимум один вариант ответа");

            builder.RuleFor(viewModel => viewModel.IsAutoAnswerOptionNumberingEnabled)
                .Must(_ => (_ || !_) && AnswerOptions.DistinctBy(answerOption => answerOption.SerialNumberInQuestion).Count() == AnswerOptions.Count)
                .When(viewModel => viewModel.isAutoAnswerOptionNumberingEnabledValidationState == ValidationState.Enabled)
                .WithMessage("Порядковые номера вариантов ответа не могут повторяться");
            
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

        private async Task<bool> IsIsAutoAnswerOptionNumberingValidAsync()
        {
            isAutoAnswerOptionNumberingEnabledValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            isAutoAnswerOptionNumberingEnabledValidationState = ValidationState.Disabled;

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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AnswerOptionEditView editView = new(answerOption!);
                    editView.ShowDialog();
                });
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
                if (!await IsContentValidAsync() || !await IsPointsCostValidAsync() || !await IsNumberOfAnswerOptionsValidAsync() ||
                    !await IsIsAutoAnswerOptionNumberingValidAsync())
                {
                    return;
                }

                Close(true);
            });
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() =>
            {
                Question.Content = questionBackup.Content;
                Question.PointsCost = questionBackup.PointsCost;
                Question.AnswerOptions = questionBackup.AnswerOptions;
                Question.NumberOfSecondsToAnswer = questionBackup.NumberOfSecondsToAnswer;
                Question.SerialNumberInTest = questionBackup.SerialNumberInTest;

                Close(false);
            });
        }
        #endregion
    }
}