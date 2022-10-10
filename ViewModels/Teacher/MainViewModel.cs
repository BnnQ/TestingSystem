using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Common;
using NeoSmart.AsyncLock;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private Models.Teacher teacher = null!;
        public Models.Teacher Teacher
        {
            get => teacher;
            set
            {
                if (teacher != value)
                {
                    teacher = value;
                    OnPropertyChanged(nameof(Teacher));
                }
            }
        }

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

        private readonly BackgroundWorker categoriesUpdaterFromDatabaseBackgroundWorker = new();


        public MainViewModel(Models.Teacher teacher)
        {
            Teacher = teacher;
            databaseContext = new TestingSystemTeacherContext();

            SetupBackgroundWorkers();
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
                Test testToBeAdded = new(new ObservableCollection<Models.Teacher>() { Teacher });
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TestEditView testEditView = new(Categories, testToBeAdded);
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
                            UpdateCategoriesFromDatabaseAsyncCommand.Execute(null);
                        }

                    }
                }
            });
        }

        private AsyncRelayCommand<Test> manageTestAsyncCommand = null!;
        public AsyncRelayCommand<Test> ManageTestAsyncCommand
        {
            get => manageTestAsyncCommand ??= new(async (test) =>
            {
                using (await databaseContextLocker.LockAsync())
                {
                    EntityEntry<Test> testEntryFromDatabase = databaseContext.Entry(test!);
                    await testEntryFromDatabase.Collection(test => test.Questions).LoadAsync();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TestInfoView testInfoView = new(databaseContext, databaseContextLocker, test!);
                        testInfoView.ShowDialog();
                    });

                }
            }, (test) => test is not null);
        }

        private RelayCommand<Category> manageCategoryCommand = null!;
        public RelayCommand<Category> ManageCategoryCommand
        {
            get => manageCategoryCommand ??= new((category) =>
            {
                CategoryInfoView categoryInfoView = new(databaseContext, databaseContextLocker, category!);
                categoryInfoView.ShowDialog();
            }, (category) => category is not null);
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