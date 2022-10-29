using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using HelperDialogs.Views;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Helpers;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class TestInfoViewModel : ViewModelBase
    {
        private Test test = null!;
        public Test Test
        {
            get => test;
            set
            {
                if (test != value)
                {
                    test = value;
                    OnPropertyChanged(nameof(Test));
                }
            }
        }
        
        private readonly Models.Teacher teacher = null!;
        public BackgroundWorker TestUpdaterFromDatabaseBackgroundWorker { get; init; } = new();

        public TestInfoViewModel(Test test, Models.Teacher teacher)
        {
            Test = test;
            this.teacher = teacher;

            SetupBackgroundWorkers();
            _ = UpdateTestFromDatabaseAsyncCommand.ExecuteAsync(null);
        }

        private void SetupBackgroundWorkers()
        {
            TestUpdaterFromDatabaseBackgroundWorker.OnWorkStarting = () => CommandManager.InvalidateRequerySuggested();
            TestUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateTestFromDatabaseAsync();
            TestUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () => CommandManager.InvalidateRequerySuggested();

            textToFileWriterBackgroundWorker.OnWorkStarting = () => Mouse.OverrideCursor = Cursors.AppStarting;
            textToFileWriterBackgroundWorker.DoWork = async (parameters) =>
            {
                if (parameters is null || parameters.Length < 1)
                    return;

                string path = parameters.First().Item1;
                bool includeCorrectAnswers = parameters.First().Item2;

                try
                {
                    await TestFileWriter.WriteToTextFileAsync(Test, path, includeCorrectAnswers);
                }
                catch (Exception exception)
                {
                    OccurErrorMessage(exception);
                    return;
                }
            };
            textToFileWriterBackgroundWorker.OnWorkCompleted = () => Mouse.OverrideCursor = Cursors.Arrow;
        }

        private async Task UpdateTestFromDatabaseAsync()
        {
            try
            {
                using (TestingSystemTeacherContext context = new())
                {
                    Test = (await context.FindAsync<Test>(Test.Id))!;
                    if (Test is null)
                        throw new NullReferenceException("Во время редактирования теста он был параллельно удалён другим пользователем или системой.");

                    EntityEntry<Test> testEntry = context.Entry(Test!);

                    await testEntry.Collection(test => test.Questions).LoadAsync();
                    foreach (Question question in Test!.Questions)
                        await context.Entry(question).Collection(question => question.AnswerOptions).LoadAsync();
                    
                    await testEntry.Collection(test => test.OwnerTeachers).LoadAsync();
                }
            }
            catch (Exception exception)
            {
                OccurCriticalErrorMessage(exception);
                return;
            }
        }


        #region Commands
        private bool IsTeacherOwner()
        {
            return Test.OwnerTeachers.Any(t => t.Id == teacher.Id);
        }

        private AsyncRelayCommand updateTestFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateTestFromDatabaseAsyncCommand
        {
            get => updateTestFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!TestUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await TestUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
            });
        }

        private AsyncRelayCommand editTestAsyncCommand = null!;
        public AsyncRelayCommand EditTestAsyncCommand
        {
            get => editTestAsyncCommand ??= new(async () =>
            {
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TestEditView testEditView = new(Test!);
                    editViewDialogResult = testEditView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    await UpdateTestFromDatabaseAsyncCommand.ExecuteAsync(null);
            }, () => !isRemoveLocked && Test is not null && IsTeacherOwner());
        }

        private bool isRemoveLocked = false;
        private AsyncRelayCommand removeTestAsyncCommand = null!;
        public AsyncRelayCommand RemoveTestAsyncCommand
        {
            get => removeTestAsyncCommand ??= new(async () =>
            {
                ConfirmationDialogView confirmationDialog = new(
                    warningMessage: "Вы уверены что хотите удалить тест?",
                    descriptionMessage: "Это действие нельзя будет отменить.",
                    cancelText: "Оставить тест",
                    confirmText: "Удалить тест");

                bool? confirmationDialogResult = default;
                Application.Current?.Dispatcher.Invoke(() => confirmationDialogResult = confirmationDialog.ShowDialog());
                if (confirmationDialogResult == true)
                {
                    isRemoveLocked = true;
                    try
                    {
                        using (TestingSystemTeacherContext context = new())
                        {
                            context.Tests.Remove(Test!);
                            await context.SaveChangesAsync();

                            Close(true);
                        }
                    }
                    catch (Exception exception)
                    {
                        OccurCriticalErrorMessage(exception);
                        return;
                    }
                }

            }, () => !isRemoveLocked && Test is not null && IsTeacherOwner());
        }

        #region PopupBox
        private readonly BackgroundWorker<Tuple<string, bool>> textToFileWriterBackgroundWorker = new();
        private async Task SaveTestToTextFileAsync(bool includeCorrectAnswers = false)
        {
            VistaSaveFileDialog saveFileDialog = new();
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Title = "Сохранение теста в текстовый файл";

            bool? saveFileDialogResult = default;
            Application.Current?.Dispatcher.Invoke(() => saveFileDialogResult = saveFileDialog.ShowDialog());

            if (saveFileDialogResult == true)
            {
                string path = saveFileDialog.FileName;
                await textToFileWriterBackgroundWorker.RunWorkerAsync(new Tuple<string, bool>(path, includeCorrectAnswers));
            }
        }
        private AsyncRelayCommand saveTestToTextFileCommand = null!;
        public AsyncRelayCommand SaveTestToTextFileCommand
        {
            get => saveTestToTextFileCommand ??= new(async () => await SaveTestToTextFileAsync());
        }

        private AsyncRelayCommand saveTestWithAnswersToTextFileCommand = null!;
        public AsyncRelayCommand SaveTestWithAnswersToTextFileCommand
        {
            get => saveTestWithAnswersToTextFileCommand ??= new(
                async () => await SaveTestToTextFileAsync(true),
                () => IsTeacherOwner());
        }
        #endregion
        #endregion


    }
}