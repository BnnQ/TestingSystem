using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common;
using NeoSmart.AsyncLock;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;

namespace TestingSystem.ViewModels.Teacher
{
    public class AnswerOptionEditViewModel : ViewModelBase
    {
        private readonly AnswerOption answerOption;


        private ushort serialNumberInQuestion;
        public ushort SerialNumberInQuestion
        {
            get => serialNumberInQuestion;
            set
            {
                if (serialNumberInQuestion != value)
                {
                    serialNumberInQuestion = value;
                    OnPropertyChanged(nameof(SerialNumberInQuestion));
                }
            }
        }

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

        private bool isCorrect;
        public bool IsCorrect
        {
            get => isCorrect;
            set
            {
                if (isCorrect != value)
                {
                    isCorrect = value;
                    OnPropertyChanged(nameof(IsCorrect));
                }
            }
        }

        private readonly TestingSystemTeacherContext databaseContext;
        private readonly AsyncLock databaseContextLocker;


        public AnswerOptionEditViewModel(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker, AnswerOption answerOption)
        {
            this.answerOption = answerOption;
            this.databaseContext = databaseContext;
            this.databaseContextLocker = databaseContextLocker;

            SerialNumberInQuestion = answerOption.SerialNumberInQuestion;
            Content = answerOption.Content;
            IsCorrect = answerOption.IsCorrect;
        }


        #region Commands
        private RelayCommand okCommand = null!;
        public RelayCommand OkCommand
        {
            get => okCommand ??= new(() =>
            {
                answerOption.SerialNumberInQuestion = SerialNumberInQuestion;
                answerOption.Content = Content;
                answerOption.IsCorrect = IsCorrect;

                Close(true);
            }, () => SerialNumberInQuestion > 0 && !string.IsNullOrWhiteSpace(Content));
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion


    }
}