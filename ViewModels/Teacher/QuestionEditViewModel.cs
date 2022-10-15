using HappyStudio.Mvvm.Input.Wpf;
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

namespace TestingSystem.ViewModels.Teacher
{
    public class QuestionEditViewModel : ValidatableViewModelBase
    {
        private Question question;


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

        private ObservableCollection<AnswerOption> answerOptions = null!;
        public ObservableCollection<AnswerOption> AnswerOptions
        {
            get => answerOptions;
            set
            {
                if (answerOptions != value)
                {
                    answerOptions = value;
                    OnPropertyChanged(nameof(AnswerOptions));
                    OnPropertyChanged(nameof(NumberOfAnswerOptions));
                }
            }
        }

        private ushort answerOptionsSeed = 0;
        public ushort NumberOfAnswerOptions
        {
            get => (ushort) AnswerOptions.Count;
            set
            {
                if (AnswerOptions.Count == value)
                    return;

                if (AnswerOptions.Count > value)
                {
                    AnswerOptions = new ObservableCollection<AnswerOption>(AnswerOptions.Take(value));
                }
                else if (AnswerOptions.Count < value)
                {
                    while (AnswerOptions.Count < value)
                        AnswerOptions.Add(new AnswerOption(question, ++answerOptionsSeed));
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

        private bool doesQuestionExistInDatabase;

        public QuestionEditViewModel(Question question)
        {
            TestingSystemTeacherContext context = new();
            Question? questionEntity = context.Find<Question>(question.Id);
            if (questionEntity is not null)
            {
                doesQuestionExistInDatabase = true;

                EntityEntry<Question> questionEntry = context.Entry(questionEntity);
                questionEntry.Collection(question => question.AnswerOptions).Load();

                this.question = questionEntity;
                context.Dispose();
            }
            else
            {
                context.Dispose();

                doesQuestionExistInDatabase = false;
                this.question = question;
            }

            Content = this.question!.Content;
            PointsCost = this.question.PointsCost;
            AnswerOptions = new ObservableCollection<AnswerOption>(this.question.AnswerOptions);
            NumberOfAnswerOptions = this.question.NumberOfAnswerOptions;
            NumberOfSecondsToAnswer = this.question.NumberOfSecondsToAnswer;
            SerialNumberInTest = this.question.SerialNumberInTest;

            SetupValidator();
        }

        #region Validation setup
        private ValidationState contentValidationState = ValidationState.Disabled;
        private ValidationState pointsCostValidationState = ValidationState.Disabled;
        private ValidationState numberOfAnswerOptionsValidationState = ValidationState.Disabled;
        private ValidationState serialNumberInTestValidationState = ValidationState.Disabled;
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

            builder.RuleFor(viewModel => viewModel.SerialNumberInTest)
                .Must(serialNumberInTest => serialNumberInTest > 0)
                .When(viewModel => viewModel.serialNumberInTestValidationState == ValidationState.Enabled)
                .WithMessage("Порядковый номер вопроса в тесте не может быть меньше 1");

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

        private async Task<bool> IsSerialNumberInTestValidAsync()
        {
            serialNumberInTestValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            serialNumberInTestValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }
        #endregion

        #region Commands
        private RelayCommand addAnswerOptionCommand = null!;
        public RelayCommand AddAnswerOptionCommand
        {
            get => addAnswerOptionCommand ??= new(() =>
            {
                AnswerOption answerOptionToBeAdded = new(question, ++answerOptionsSeed);
                
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

        private void SaveQuestionChangesLocally()
        {
            question.Content = Content;
            question.PointsCost = PointsCost;
            question.AnswerOptions = AnswerOptions;
            question.NumberOfSecondsToAnswer = NumberOfSecondsToAnswer;
            question.SerialNumberInTest = SerialNumberInTest;
        }
        private AsyncRelayCommand confirmAsyncCommand = null!;
        public AsyncRelayCommand ConfirmAsyncCommand
        {
            get => confirmAsyncCommand ??= new(async () =>
            {
                if (!await IsContentValidAsync() || !await IsPointsCostValidAsync() ||
                    !await IsNumberOfAnswerOptionsValidAsync() || !await IsSerialNumberInTestValidAsync())
                {
                    return;
                }

                if (doesQuestionExistInDatabase)
                {
                    using (TestingSystemTeacherContext context = new())
                    {
                        question = (await context.FindAsync<Question>(question.Id))!;
                        SaveQuestionChangesLocally();
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    SaveQuestionChangesLocally();
                }

                Close(true);
            });
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion

    }
}