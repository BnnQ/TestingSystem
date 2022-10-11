using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore;
using MvvmBaseViewModels.Common;
using NeoSmart.AsyncLock;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class TestInfoViewModel : ViewModelBase
    {
        private readonly TestingSystemTeacherContext databaseContext;
        private readonly AsyncLock databaseContextLocker;

        private Category[] categories = null!;

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

        public TestInfoViewModel(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker, Test test)
        {
            this.databaseContext = databaseContext;
            this.databaseContextLocker = databaseContextLocker;
            Test = test;
        }

        #region Commands
        private AsyncRelayCommand editTestAsyncCommand = null!;
        public AsyncRelayCommand EditTestAsyncCommand
        {
            get => editTestAsyncCommand ??= new(async () =>
            {
                Test? testInDatabase = default;
                using (await databaseContextLocker.LockAsync())
                {
                    await databaseContext.Categories.LoadAsync();
                    await databaseContext.Categories
                    .Include(category => category.Tests)
                        .ThenInclude(test => test.Category)
                    .Include(category => category.Tests)
                        .ThenInclude(test => test.Questions)
                    .Include(category => category.Tests)
                        .ThenInclude(test => test.OwnerTeachers)
                    .LoadAsync();
                    
                    categories = await databaseContext.Categories.ToArrayAsync();
                    testInDatabase = await databaseContext.FindAsync<Test>(Test.Id);
                }

                if (testInDatabase is not null)
                {
                    bool? editViewDialogResult = default;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TestEditView testEditView = new(databaseContext, databaseContextLocker, categories, testInDatabase);
                        editViewDialogResult = testEditView.ShowDialog();
                    });

                    if (editViewDialogResult == true)
                    {
                        using (await databaseContextLocker.LockAsync())
                            await databaseContext.SaveChangesAsync();
                    }
                }
               
            });
        }

        private AsyncRelayCommand removeTestAsyncCommand = null!;
        public AsyncRelayCommand RemoveTestAsyncCommand
        {
            get => removeTestAsyncCommand ??= new(async () =>
            {
                using (await databaseContextLocker.LockAsync())
                {
                    Test? testToBeRemoved = await databaseContext.FindAsync<Test>(Test.Id);
                    if (testToBeRemoved is not null)
                    {
                        databaseContext.Remove(testToBeRemoved);
                        await databaseContext.SaveChangesAsync();
                    }
                }
            });
        }
        #endregion


    }
}