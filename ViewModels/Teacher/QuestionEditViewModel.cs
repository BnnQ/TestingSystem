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
    public class QuestionEditViewModel : ViewModelBase
    {
        private readonly Question question;


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

        private readonly TestingSystemTeacherContext databaseContext;
        private readonly AsyncLock databaseContextLocker;


        public QuestionEditViewModel(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker, Question question)
        {
            this.databaseContext = databaseContext;
            this.databaseContextLocker = databaseContextLocker;
            this.question = question;

            Content = question.Content;
            PointsCost = question.PointsCost;
            AnswerOptions = new ObservableCollection<AnswerOption>(question.AnswerOptions);
            NumberOfAnswerOptions = question.NumberOfAnswerOptions;
            NumberOfSecondsToAnswer = question.NumberOfSecondsToAnswer;
            SerialNumberInTest = question.SerialNumberInTest;
        }


        #region Commands
        private RelayCommand okCommand = null!;
        public RelayCommand OkCommand 
        {
            get => okCommand ??= new(() =>
            {
                question.Content = Content;
                question.PointsCost = PointsCost;
                question.AnswerOptions = AnswerOptions;
                question.NumberOfSecondsToAnswer = NumberOfSecondsToAnswer;
                question.SerialNumberInTest = SerialNumberInTest;

                Close(true);
            }, () => !string.IsNullOrWhiteSpace(Content) && PointsCost > 0 && NumberOfAnswerOptions > 0 && SerialNumberInTest > 0);
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }

        private AsyncRelayCommand<AnswerOption> editAnswerOptionAsyncCommand = null!;
        public AsyncRelayCommand<AnswerOption> EditAnswerOptionAsyncCommand
        {
            get => editAnswerOptionAsyncCommand ??= new(async (answerOption) =>
            {
                AnswerOption? answerOptionFromDatabase = default;
                using (await databaseContextLocker.LockAsync())
                    answerOptionFromDatabase = await databaseContext.FindAsync<AnswerOption>(answerOption!.Id);

                if (answerOptionFromDatabase is not null)
                {
                    bool? editViewDialogResult = default;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AnswerOptionEditView editView = new(answerOptionFromDatabase);
                        editViewDialogResult = editView.ShowDialog();
                    });

                    if (editViewDialogResult == true)
                    {
                        using (await databaseContextLocker.LockAsync())
                            await databaseContext.SaveChangesAsync();
                    }
                }

            }, (answerOption) => answerOption is not null);
        }
        
        #endregion

    }
}