using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using NeoSmart.AsyncLock;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;
using Z.Linq;

namespace TestingSystem.ViewModels.Teacher
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Models.Teacher teacher;

        private readonly AsyncLock databaseContextLocker = new();
        private readonly TestingSystemTeacherContext databaseContext;
        private Category[] categories = null!;
        public Category[] Categories
        {
            get => categories;
            set
            {
                if (categories != value)
                {
                    categories = value;
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }

        private bool isAddMenuOpen = false;
        public bool IsAddMenuOpen
        {
            get => isAddMenuOpen;
            set
            {
                if (isAddMenuOpen != value)
                {
                    isAddMenuOpen = value;
                    OnPropertyChanged(nameof(IsAddMenuOpen));
                }
            }
        }

        private readonly BackgroundWorker categoriesUpdaterFromDatabaseBackgroundWorker = new();


        public MainViewModel(Models.Teacher teacher)
        {
            this.teacher = teacher;
            databaseContext = new TestingSystemTeacherContext();

            SetupBackgroundWorkers();
            UpdateCategoriesFromDatabaseAsyncCommand.Execute(null);
        }

        private void SetupBackgroundWorkers()
        {
            categoriesUpdaterFromDatabaseBackgroundWorker.DoWork = async () =>
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
                await UpdateCategoriesFromDatabaseAsync();
            };
            categoriesUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            };

        }

        private async Task UpdateCategoriesFromDatabaseAsync()
        {
            using (await databaseContextLocker.LockAsync())
            {
                await databaseContext.Categories.LoadAsync();
                await databaseContext.Categories
                    .Include(category => category.Tests)
                    .LoadAsync();

                Categories = await databaseContext.Categories.ToArrayAsync();
            }
        }

        #region Commands
        private AsyncRelayCommand updateCategoriesFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateCategoriesFromDatabaseAsyncCommand
        {
            get => updateCategoriesFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!categoriesUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await categoriesUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
            });
        }

        private RelayCommand openAddMenuCommand = null!;
        public RelayCommand OpenAddMenuCommand
        {
            get => openAddMenuCommand ??= new(() => IsAddMenuOpen = !IsAddMenuOpen);
        }

        private AsyncRelayCommand addCategoryAsyncCommand = null!;
        public AsyncRelayCommand AddCategoryAsyncCommand
        {
            get => addCategoryAsyncCommand ??= new(async () =>
            {
                Category categoryToBeAdded = new();
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CategoryEditView categoryEditView = new(categoryToBeAdded);
                    editViewDialogResult = categoryEditView.ShowDialog();
                });

                if (editViewDialogResult == true)
                {
                    using (await databaseContextLocker.LockAsync())
                    {
                        await databaseContext.Categories.AddAsync(categoryToBeAdded);
                        await databaseContext.SaveChangesAsync();
                        UpdateCategoriesFromDatabaseAsyncCommand.Execute(null);
                    }
                }

            });
        }

        private AsyncRelayCommand addTestAsyncCommand = null!;
        public AsyncRelayCommand AddTestAsyncCommand
        {
            get => addTestAsyncCommand ??= new(async () =>
            {
                Test testToBeAdded = new(new ObservableCollection<Models.Teacher>() { teacher });
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TestEditView testEditView = new(databaseContext, databaseContextLocker, Categories, testToBeAdded);
                    editViewDialogResult = testEditView.ShowDialog();
                });

                if (editViewDialogResult == true)
                {
                    using (await databaseContextLocker.LockAsync())
                    {
                        Category? categoryToWhichTestBeAdded =
                        await databaseContext.FindAsync<Category>(testToBeAdded.CategoryId);

                        if (categoryToWhichTestBeAdded is not null)
                        {
                            categoryToWhichTestBeAdded.Tests.Add(testToBeAdded);
                            await databaseContext.SaveChangesAsync();
                            await UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
                        }
                        
                    }
                }
            });
        }

        private AsyncRelayCommand<Category> manageCategoryAsyncCommand = null!;
        public AsyncRelayCommand<Category> ManageCategoryAsyncCommand
        {
            get => manageCategoryAsyncCommand ??= new(async (category) =>
            {
                Category? categoryEntityFromDatabase = default;
                using (await databaseContextLocker.LockAsync())
                {
                    categoryEntityFromDatabase = await databaseContext.FindAsync<Category>(category!.Id);
                    if (categoryEntityFromDatabase is not null)
                    {
                        await databaseContext.Entry(categoryEntityFromDatabase)
                        .Collection(category => category.Tests)
                        .LoadAsync();
                    }
                }

                if (categoryEntityFromDatabase is not null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CategoryInfoView categoryInfoView = new(databaseContext, databaseContextLocker, categoryEntityFromDatabase, teacher);
                        categoryInfoView.ShowDialog();
                    });

                    using (await databaseContextLocker.LockAsync())
                        Categories = await databaseContext.Categories.ToArrayAsync();
                }
            }, (category) => category is not null);
        }

        private AsyncRelayCommand<Test> manageTestAsyncCommand = null!;
        public AsyncRelayCommand<Test> ManageTestAsyncCommand
        {
            get => manageTestAsyncCommand ??= new(async (test) =>
            {
                Test? testEntityFromDatabase = default;
                using (await databaseContextLocker.LockAsync())
                {
                    testEntityFromDatabase = await databaseContext.FindAsync<Test>(test!.Id);
                    if (testEntityFromDatabase is not null)
                    {
                        EntityEntry<Test> testEntry = databaseContext.Entry(testEntityFromDatabase);
                        
                        await testEntry
                        .Collection(test => test.Questions)
                        .LoadAsync();

                        await testEntry
                        .Collection(test => test.OwnerTeachers)
                        .LoadAsync();
                    }
                }
                
                if (testEntityFromDatabase is not null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TestInfoView testInfoView = new(databaseContext, databaseContextLocker, testEntityFromDatabase, teacher);
                        testInfoView.ShowDialog();
                    });


                    using (await databaseContextLocker.LockAsync())
                        Categories = await databaseContext.Categories.ToArrayAsync();
                }

            }, (test) => test is not null);
        }
        #endregion

        #region Closing and disposing
        public override void Close(bool? dialogResult = null)
        {
            Dispose();
            base.Close(dialogResult);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed = false;
        protected virtual void Dispose(bool needDisposing)
        {
            if (isDisposed)
                return;

            if (needDisposing)
            {
                using (databaseContextLocker.Lock())
                    databaseContext?.Dispose();
            }

            isDisposed = true;
        }

        ~MainViewModel() => Dispose(false);
        #endregion
    }
}