using BackgroundWorkerLibrary;
using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
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
using TestingSystem.Views.Student;

namespace TestingSystem.ViewModels.Student
{
    public class MainViewModel : NavigationViewModelBase
    {
        private readonly static IEqualityComparer<Category> categoryEqualityComparer = new CategoryByIdEqualityComparer();
        private readonly static IEqualityComparer<Test> testEqualityComparer = new TestByIdEqualityComparer();

        private Models.Student student = null!;

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
        public BackgroundWorker<Models.Student> InitialLoaderBackgroundWorker { get; init; } = new();

        public MainViewModel(INavigationManager navigationManager, Models.Student student) : base(navigationManager)
        {
            SetupBackgroundWorkers();
            _ = InitialLoaderBackgroundWorker.RunWorkerAsync(student);
            _ = UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
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

                Models.Student student = parameters!.First();
                try
                {
                    using (TestingSystemStudentContext context = new())
                    {
                        Models.Student? studentEntity = await context.FindAsync<Models.Student>(student.Id);
                        if (studentEntity is null)
                            throw new NullReferenceException("Student entity missing from the database (most likely, a problem on the DB side)");
                        else
                            this.student = studentEntity;
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
                using (TestingSystemStudentContext context = new())
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

        private RelayCommand<Test> openTestCommand = null!;
        public RelayCommand<Test> OpenTestCommand
        {
            get => openTestCommand ??= new((test) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    TestInfoView infoView = new(test!, student);
                    infoView.ShowDialog();
                });

                _ = UpdateCategoriesFromDatabaseAsyncCommand.ExecuteAsync(null);
            }, (test) => test is not null);
        }
        #endregion

    }
}