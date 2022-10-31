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
using TestingSystem.Helpers.Comparers;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;

namespace TestingSystem.ViewModels.Student
{
    public class StatisticsViewModel : NavigationViewModelBase
    {
        private readonly static IComparer<TestResult> testResultComparer = new TestResultByCompletionDateDescendingComparer();

        private readonly static IEqualityComparer<Test> testEqualityComparer = new TestByIdEqualityComparer();

        private readonly Models.Student student;

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

        private Test? selectedTest;
        public Test? SelectedTest
        {
            get => selectedTest;
            set
            {
                if (selectedTest != value)
                {
                    selectedTest = value;
                    OnPropertyChanged(nameof(SelectedTest));

                    _ = UpdateStatisticsAsyncCommand.ExecuteAsync(null);
                }
            }
        }

        private DateTime? minimumDate;
        public DateTime? MinimumDate
        {
            get => minimumDate;
            set
            {
                if (minimumDate != value)
                {
                    minimumDate = value;
                    OnPropertyChanged(nameof(MinimumDate));

                    _ = UpdateStatisticsAsyncCommand.ExecuteAsync(null);
                }
            }
        }

        private ImmutableSortedSet<TestResult> statistics = null!;
        public ImmutableSortedSet<TestResult> Statistics
        {
            get => statistics;
            set
            {
                if (statistics != value)
                {
                    if (value.KeyComparer != testResultComparer)
                        value = value.WithComparer(testResultComparer);

                    statistics = value;
                    OnPropertyChanged(nameof(Statistics));
                }
            }
        }

        public BackgroundWorker DataUpdaterFromDatabaseBackgroundWorker { get; init; } = new();
        public BackgroundWorker StatisticsUpdaterBackgroundWorker { get; init; } = new();


        public StatisticsViewModel(INavigationManager navigationManager, Models.Student student) : base(navigationManager, isRecursiveClosingOfNagivationParentsEnabled: true)
        {
            this.student = student;

            SetupBackgroundWorkers();
            Task initialUpdatingTask = UpdateDataFromDatabaseAsyncCommand.ExecuteAsync(null);
            initialUpdatingTask.ContinueWith((_) => UpdateStatisticsAsyncCommand.ExecuteAsync(null));
        }

        private void SetupBackgroundWorkers()
        {
            DataUpdaterFromDatabaseBackgroundWorker.DoWork = async () =>
            {
                using (TestingSystemStudentContext context = new())
                {
                    try
                    {
                        await context.Tests.LoadAsync();
                        Tests = context.Tests.ToImmutableHashSet(testEqualityComparer);
                    }
                    catch (Exception exception)
                    {
                        OccurCriticalErrorMessage(exception);
                        return;
                    }
                }

                await UpdateStatisticsAsyncCommand.ExecuteAsync(null);
            };

            StatisticsUpdaterBackgroundWorker.DoWork = async () =>
            {
                try
                {
                    Statistics = await GetStatisticsAsync(new TestResultByCompletionDateDescendingComparer());
                }
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }
            };

        }

        #region Getting statistics
        private Task<ImmutableSortedSet<TestResult>> GetStatisticsAsync(IComparer<TestResult> statisticsSortComparer)
        {
            if (SelectedTest is not null)
                return GetStatisticsByTestAsync(SelectedTest, statisticsSortComparer);
            else
                return GetAllStatisticsAsync(statisticsSortComparer);
        }

        private async Task<ImmutableSortedSet<TestResult>> GetAllStatisticsAsync(IComparer<TestResult> statisticsSortComparer)
        {
            using (TestingSystemStudentContext context = new())
            {
                if (MinimumDate.HasValue)
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Student.Id == student.Id
                                                      && testResult.CompletionDate > MinimumDate)
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();
                }
                else
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Student.Id == student.Id)
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();
                }

                return await Task.Run(() => context.TestResults.Local.ToImmutableSortedSet(statisticsSortComparer));
            }
        }

        private async Task<ImmutableSortedSet<TestResult>> GetStatisticsByTestAsync(Test test, IComparer<TestResult> statisticsSortComparer)
        {
            using (TestingSystemStudentContext context = new())
            {
                if (MinimumDate.HasValue)
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Student.Id == student.Id
                                                      && testResult.Test.Id == test.Id
                                                      && testResult.CompletionDate > MinimumDate)
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();
                }
                else
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Student.Id == student.Id
                                                      && testResult.Test.Id == test.Id)
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();
                }

                return await Task.Run(() => context.TestResults.Local.ToImmutableSortedSet(statisticsSortComparer));
            }
        }
        #endregion

        #region Commands
        private AsyncRelayCommand updateDataFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateDataFromDatabaseAsyncCommand
        {
            get => updateDataFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!DataUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await DataUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
            });
        }

        private AsyncRelayCommand updateStatisticsAsyncCommand = null!;
        public AsyncRelayCommand UpdateStatisticsAsyncCommand
        {
            get => updateStatisticsAsyncCommand ??= new(async () =>
            {
                if (!StatisticsUpdaterBackgroundWorker.IsBusy)
                    await StatisticsUpdaterBackgroundWorker.RunWorkerAsync();
            });
        }
        #endregion

    }
}