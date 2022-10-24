using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModels.Enums;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System.Threading.Tasks;
using TestingSystem.Models;

namespace TestingSystem.ViewModels.Teacher
{
    public class AnswerOptionEditViewModel : ValidatableViewModelBase
    {
        private readonly AnswerOption answerOptionBackup;

        private void OnAnswerOptionChanged(object? _, System.ComponentModel.PropertyChangedEventArgs args) => OnPropertyChanged(args.PropertyName);
        private AnswerOption answerOption = null!;
        public AnswerOption AnswerOption
        {
            get => answerOption;
            set
            {
                if (answerOption != value)
                {
                    if (answerOption is not null)
                        answerOption.PropertyChanged -= OnAnswerOptionChanged;

                    answerOption = value;
                    OnPropertyChanged(nameof(AnswerOption));

                    if (answerOption is not null)
                        answerOption.PropertyChanged += OnAnswerOptionChanged;
                }
            }
        }

        public ushort SerialNumberInQuestion
        {
            get => AnswerOption.SerialNumberInQuestion;
            set => AnswerOption.SerialNumberInQuestion = value;
        }

        public string Content
        {
            get => AnswerOption.Content;
            set => AnswerOption.Content = value;
        }

        public bool IsCorrect
        {
            get => AnswerOption.IsCorrect;
            set => AnswerOption.IsCorrect = value;
        }


        public AnswerOptionEditViewModel(AnswerOption answerOption)
        {
            AnswerOption = answerOption;
            answerOptionBackup = new(answerOption.Question, answerOption.SerialNumberInQuestion,
                 answerOption.Content, answerOption.IsCorrect);

            SetupValidator();
        }

        #region Validaiton setup
        ValidationState contentValidationState = ValidationState.Disabled;
        protected override void SetupValidator()
        {
            ValidationBuilder<AnswerOptionEditViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Content)
                .Must(content => !string.IsNullOrWhiteSpace(content))
                .When(viewModel => viewModel.contentValidationState == ValidationState.Enabled)
                .WithMessage("Вариант ответа не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Content)
                .MaxLength(255)
                .When(viewModel => viewModel.contentValidationState == ValidationState.Enabled)
                .WithMessage("Вариант ответа не может быть длиннее 255 символов");

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
        #endregion

        #region Commands
        private AsyncRelayCommand confirmAsyncCommand = null!;
        public AsyncRelayCommand ConfirmAsyncCommand
        {
            get => confirmAsyncCommand ??= new(async () =>
            {
                if (!await IsContentValidAsync())
                    return;

                Close(true);
            });
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() =>
            {
                AnswerOption.Content = answerOptionBackup.Content;
                AnswerOption.SerialNumberInQuestion = answerOptionBackup.SerialNumberInQuestion;
                AnswerOption.IsCorrect = answerOptionBackup.IsCorrect;

                Close(false);
            });
        }
        #endregion
    }
}