using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Helpers;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Student;

namespace TestingSystem.ViewModels.Student
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

        private readonly Models.Student student = null!;
        public BackgroundWorkerLibrary.BackgroundWorker TestUpdaterFromDatabaseBackgroundWorker { get; init; } = new();

        public TestInfoViewModel(Test test, Models.Student student)
        {
            Test = test;
            this.student = student;

            SetupBackgroundWorkers();
            _ = UpdateTestFromDatabaseAsyncCommand.ExecuteAsync(null);
        }

        private void SetupBackgroundWorkers()
        {
            TestUpdaterFromDatabaseBackgroundWorker.OnWorkStarting = () => CursorOverrider.OverrideCursorCommand.Execute(Cursors.Wait);
            TestUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateTestFromDatabaseAsync();
            TestUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () =>
            {
                CursorOverrider.OverrideCursorCommand.Execute(Cursors.Arrow);
                CommandManager.InvalidateRequerySuggested();
            };
        }

        private async Task UpdateTestFromDatabaseAsync()
        {
            try
            {
                using (TestingSystemStudentContext context = new())
                {
                    Test = (await context.FindAsync<Test>(Test.Id))!;
                    if (Test is null)
                        throw new NullReferenceException("Тест недоступен, поскольку с момента последнего обновления был удалён учителем или системой.");

                    EntityEntry<Test> testEntry = context.Entry(Test!);

                    await testEntry.Collection(test => test.Questions).LoadAsync();
                    foreach (Question question in Test!.Questions)
                        await context.Entry(question).Collection(question => question.AnswerOptions).LoadAsync();
                }
            }
            catch (Exception exception)
            {
                OccurErrorMessage(exception);
                return;
            }
        }

        #region Commands
        private AsyncRelayCommand updateTestFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateTestFromDatabaseAsyncCommand
        {
            get => updateTestFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!TestUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await TestUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
            });
        }

        private RelayCommand startTestCommand = null!;
        public RelayCommand StartTestCommand
        {
            get => startTestCommand ??= new(() =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    TestContainerView testContainerView = new(Test, student);
                    testContainerView.ShowDialog();
                });
            }, () => !TestUpdaterFromDatabaseBackgroundWorker.IsBusy);
        }
        #endregion
    }
}