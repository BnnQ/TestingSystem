﻿using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class TestInfoViewModel : ViewModelBase
    {
        private Test? test = null!;
        public Test? Test
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
        
        private readonly Models.Teacher teacher;
        private readonly BackgroundWorkerLibrary.BackgroundWorker testUpdaterFromDatabaseBackgroundWorker = new();

        public TestInfoViewModel(Test test, Models.Teacher teacher)
        {
            using (TestingSystemTeacherContext context = new())
            {
                Test? testEntity = context.Find<Test>(test.Id);
                if (testEntity is null)
                {
                    OccurCriticalErrorMessage("Test entity missing from the database (most likely, a problem on the DB side)");
                    Close();
                }
                else
                {
                    Test = testEntity;
                }
            }

            this.teacher = teacher;
            SetupBackgroundWorkers();

            _ = UpdateTestFromDatabaseAsyncCommand.ExecuteAsync(null);
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
            if (Test is not null)
            {
                using (TestingSystemTeacherContext context = new())
                {
                    Test = await context.FindAsync<Test>(Test.Id);
                    if (Test is null)
                    {
                        OccurErrorMessage("Во время редактирования теста он был параллельно удалён другим пользователем или системой.");
                        Close();
                    }

                    EntityEntry<Test> testEntry = context.Entry(Test!);

                    await testEntry.Collection(test => test.Questions).LoadAsync();
                    foreach (Question question in Test!.Questions)
                        await context.Entry(question).Collection(question => question.AnswerOptions).LoadAsync();

                    await testEntry.Collection(test => test.OwnerTeachers).LoadAsync();
                }
            }
        }

        #region Commands
        private bool IsTeacherOwner()
        {
            if (Test is null)
                return false;
            else
                return Test.OwnerTeachers.Any(t => t.Id == teacher.Id);
        }

        private AsyncRelayCommand updateTestFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateTestFromDatabaseAsyncCommand
        {
            get => updateTestFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!testUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await testUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
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
            }, () => Test is not null && IsTeacherOwner() && !testUpdaterFromDatabaseBackgroundWorker.IsBusy);
        }

        private AsyncRelayCommand removeTestAsyncCommand = null!;
        public AsyncRelayCommand RemoveTestAsyncCommand
        {
            get => removeTestAsyncCommand ??= new(async () =>
            {
                using (TestingSystemTeacherContext context = new())
                {
                    context.Tests.Remove(Test!);
                    await context.SaveChangesAsync();

                    Close(true);
                }
            }, () => Test is not null && IsTeacherOwner() && !testUpdaterFromDatabaseBackgroundWorker.IsBusy);
        }
        #endregion


    }
}