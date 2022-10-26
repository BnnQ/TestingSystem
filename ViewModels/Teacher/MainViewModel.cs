using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Meziantou.Framework.WPF.Collections;
using Microsoft.EntityFrameworkCore;
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
    public class MainViewModel : ViewModelBase
    {
        private Models.Teacher teacher = null!;

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
        private Models.Teacher[] teachers = null!;
        public Models.Teacher[] Teachers
        {
            get => teachers;
            set
            {
                if (teachers != value)
                {
                    teachers = value;
                    OnPropertyChanged(nameof(Teachers));
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

        public BackgroundWorker CategoriesUpdaterFromDatabaseBackgroundWorker { get; init; } = new();
        public BackgroundWorker<Models.Teacher> InitialLoaderBackgroundWorker { get; init; } = new();

        public MainViewModel(Models.Teacher teacher)
        {
            SetupBackgroundWorkers();
            _ = InitialLoaderBackgroundWorker.RunWorkerAsync(teacher);
            UpdateCategoriesFromDatabaseAsyncCommand.Execute(null);
        }

        private void SetupBackgroundWorkers()
        {
            CategoriesUpdaterFromDatabaseBackgroundWorker.MinimumWorkExecutionTime = 500;
            CategoriesUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateCategoriesFromDatabaseAsync();
            CategoriesUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () => CommandManager.InvalidateRequerySuggested();
            InitialLoaderBackgroundWorker.DoWork = async (parameters) =>
            {
                if (parameters?.Length < 1)
                    return;

                Models.Teacher teacher = parameters!.First();
                try
                {
                    using (TestingSystemTeacherContext context = new())
                    {
                        Models.Teacher? teacherEntity = await context.FindAsync<Models.Teacher>(teacher.Id);
                        if (teacherEntity is null)
                            throw new NullReferenceException("Teacher entity missing from the database (most likely, a problem on the DB side)");
                        else
                            this.teacher = teacherEntity;
                    }
                }
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }
            };
        }

        private async Task UpdateCategoriesFromDatabaseAsync()
        {
            try
            {
                using (TestingSystemTeacherContext context = new())
                {
                    await context.Categories
                        .Include(category => category.Tests)
                            .ThenInclude(test => test.Questions)
                        .LoadAsync();

                    Categories = await context.Categories.ToArrayAsync();
                }
            }
            catch (Exception exception)
            {
                OccurCriticalErrorMessage(exception);
                return;
            }
        }

        #region Commands
        private AsyncRelayCommand updateCategoriesFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateCategoriesFromDatabaseAsyncCommand
        {
            get => updateCategoriesFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!CategoriesUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await CategoriesUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
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
                    await UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
            });
        }

        private AsyncRelayCommand addTestAsyncCommand = null!;
        public AsyncRelayCommand AddTestAsyncCommand
        {
            get => addTestAsyncCommand ??= new(async () =>
            {
                Test testToBeAdded = new(new ConcurrentObservableCollection<Models.Teacher>() { teacher });
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TestEditView testEditView = new(testToBeAdded);
                    editViewDialogResult = testEditView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    await UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
            });
        }

        private AsyncRelayCommand<Category> manageCategoryAsyncCommand = null!;
        public AsyncRelayCommand<Category> ManageCategoryAsyncCommand
        {
            get => manageCategoryAsyncCommand ??= new(async (category) =>
            {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CategoryInfoView categoryInfoView = new(category!, teacher);
                        categoryInfoView.ShowDialog();
                    });
                
                await UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
            }, (category) => category is not null && !CategoriesUpdaterFromDatabaseBackgroundWorker.IsBusy);
        }

        private AsyncRelayCommand<Test> manageTestAsyncCommand = null!;
        public AsyncRelayCommand<Test> ManageTestAsyncCommand
        {
            get => manageTestAsyncCommand ??= new(async (test) =>
            {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TestInfoView testInfoView = new(test!, teacher);
                        testInfoView.ShowDialog();
                    });

                await UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
            }, (test) => test is not null && !CategoriesUpdaterFromDatabaseBackgroundWorker.IsBusy);
        }
        #endregion
    }
}