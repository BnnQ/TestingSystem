using BackgroundWorkerLibrary;
using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
using Meziantou.Framework.WPF.Collections;
using Microsoft.EntityFrameworkCore;
using MvvmBaseViewModels.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Helpers.Comparers;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class MainViewModel : NavigationViewModelBase
    {
        private readonly static IEqualityComparer<Category> categoryEqualityComparer = new CategoryByIdEqualityComparer();
        private readonly static IEqualityComparer<Test> testEqualityComparer = new TestByIdEqualityComparer();

        private Models.Teacher teacher = null!;

        private ImmutableHashSet<Category> categories = null!;
        public ImmutableHashSet<Category> Categories
        {
            get => categories;
            set
            {
                if (categories != value)
                {
                    if (value.KeyComparer != categoryEqualityComparer)
                        value = value.WithComparer(categoryEqualityComparer);

                    categories = value;
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }
        private ImmutableHashSet<Test> tests = null!;
        public ImmutableHashSet<Test> Tests
        {
            get => tests;
            set
            {
                if (tests != value)
                {
                    if (value.KeyComparer != testEqualityComparer)
                        value = value.WithComparer(testEqualityComparer);

                    tests = value;
                    OnPropertyChanged(nameof(Tests));
                }
            }
        }

        public BackgroundWorker CategoriesUpdaterFromDatabaseBackgroundWorker { get; init; } = new();
        public BackgroundWorker<Models.Teacher> InitialLoaderBackgroundWorker { get; init; } = new();

        public MainViewModel(INavigationManager navigationManager, Models.Teacher teacher) : base(navigationManager)
        {
            SetupBackgroundWorkers();
            _ = InitialLoaderBackgroundWorker.RunWorkerAsync(teacher);
            _ = UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
        }

        private void SetupBackgroundWorkers()
        {
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

            CategoriesUpdaterFromDatabaseBackgroundWorker.MinimumWorkExecutionTime = 500;
            CategoriesUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateCategoriesFromDatabaseAsync();
            CategoriesUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () => CommandManager.InvalidateRequerySuggested();
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
                    Categories = await Task.Run(() => context.Categories.ToImmutableHashSet(categoryEqualityComparer));
                    
                    await context.Tests.LoadAsync();
                    Tests = await Task.Run(() => context.Tests.ToImmutableHashSet(testEqualityComparer));
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

        #region Popup (AddMenu)
        private AsyncRelayCommand addCategoryAsyncCommand = null!;
        public AsyncRelayCommand AddCategoryAsyncCommand
        {
            get => addCategoryAsyncCommand ??= new(async () =>
            {
                Category categoryToBeAdded = new();
                bool? editViewDialogResult = default;
                Application.Current?.Dispatcher.Invoke(() =>
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
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    TestEditView testEditView = new(testToBeAdded);
                    editViewDialogResult = testEditView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    await UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
            });
        }
        #endregion

        private AsyncRelayCommand<Category> manageCategoryAsyncCommand = null!;
        public AsyncRelayCommand<Category> ManageCategoryAsyncCommand
        {
            get => manageCategoryAsyncCommand ??= new(async (category) =>
            {
                    Application.Current?.Dispatcher.Invoke(() =>
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
                    Application.Current?.Dispatcher.Invoke(() =>
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