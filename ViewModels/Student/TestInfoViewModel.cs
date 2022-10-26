﻿using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        private readonly BackgroundWorkerLibrary.BackgroundWorker testUpdaterFromDatabaseBackgroundWorker = new();

        public TestInfoViewModel(Test test, Models.Student student)
        {
            SetupBackgroundWorkers();

            Test = test;
            _ = UpdateTestFromDatabaseAsyncCommand.ExecuteAsync(null);
            

            this.student = student;
        }

        private void SetupBackgroundWorkers()
        {
            testUpdaterFromDatabaseBackgroundWorker.OnWorkStarting = () => Mouse.OverrideCursor = Cursors.Wait;
            testUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateTestFromDatabaseAsync();
            testUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
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
                if (!testUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await testUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
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
            }, () => !testUpdaterFromDatabaseBackgroundWorker.IsBusy);
        }
        #endregion
    }
}