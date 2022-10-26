﻿using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using System;
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
        
        private readonly Models.Teacher teacher = null!;
        public BackgroundWorkerLibrary.BackgroundWorker TestUpdaterFromDatabaseBackgroundWorker { get; init; } = new();

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
        }

        private async Task UpdateTestFromDatabaseAsync()
        {
            try
            {
                using (TestingSystemTeacherContext context = new())
                {
                    Test = await context.FindAsync<Test>(Test!.Id);
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
            }, () => !isRemoveLocked && Test is not null && IsTeacherOwner());
        }
        #endregion


    }
}