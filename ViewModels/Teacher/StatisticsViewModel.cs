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

namespace TestingSystem.ViewModels.Teacher
{
    public class StatisticsViewModel : NavigationViewModelBase
    {
        private readonly Models.Teacher teacher;

        private ImmutableHashSet<Models.Student> students = null!;
        public ImmutableHashSet<Models.Student> Students
        {
            get => students;
            set
            {
                if (students != value)
                {
                    students = value;
                    OnPropertyChanged(nameof(Students));
                }
            }
        }

        private Models.Student? selectedStudent;
        public Models.Student? SelectedStudent
        {
            get => selectedStudent;
            set
            {
                if (selectedStudent != value)
                {
                    selectedStudent = value;
                    OnPropertyChanged(nameof(SelectedStudent));

                    _ = UpdateStatisticsAsyncCommand.ExecuteAsync(null);
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

        private int numberOfTestsCreated;
        public int NumberOfTestsCreated
        {
            get => numberOfTestsCreated;
            set
            {
                if (value != numberOfTestsCreated)
                {
                    numberOfTestsCreated = value;
                    OnPropertyChanged(nameof(NumberOfTestsCreated));
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
                    statistics = value;
                    OnPropertyChanged(nameof(Statistics));
                }
            }
        }


        public BackgroundWorker DataUpdaterFromDatabaseBackgroundWorker { get; init; } = new();
        public BackgroundWorker StatisticsUpdaterBackgroundWorker { get; init; } = new();

        public StatisticsViewModel(INavigationManager navigationManager, Models.Teacher teacher) : base(navigationManager, isRecursiveClosingOfNagivationParentsEnabled: true)
        {
            this.teacher = teacher;

            SetupBackgroundWorkers();
            Task initialUpdatingTask = UpdateDataFromDatabaseAsyncCommand.ExecuteAsync(null);
            initialUpdatingTask.ContinueWith((_) => UpdateStatisticsAsyncCommand.ExecuteAsync(null));
        }

        private void SetupBackgroundWorkers()
        {
            DataUpdaterFromDatabaseBackgroundWorker.DoWork = async () =>
            {
                using (TestingSystemTeacherContext context = new())
                {
                    try
                    {
                        await context.Students.LoadAsync();
                        Students = context.Students.ToImmutableHashSet();

                        await context.Tests.LoadAsync();
                        Tests = context.Tests.ToImmutableHashSet();

                        NumberOfTestsCreated = context.Tests
                                                      .Where(test => test.OwnerTeachers.Contains(teacher))
                                                      .Count();
                    }
                    catch (Exception exception)
                    {
                        OccurCriticalErrorMessage(exception);
                        return;
                    }
                }
            };

            StatisticsUpdaterBackgroundWorker.DoWork = async () =>
            {
                try
                {
                    Statistics = await GetStatisticsAsync(new TestResultByCompletionDateComparer());
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
            if (SelectedStudent is null && SelectedTest is null)
                return GetAllStatisticsAsync(statisticsSortComparer);
            else if (SelectedStudent is not null)
                return GetStatisticsByStudentAsync(SelectedStudent, statisticsSortComparer);
            else if (SelectedTest is not null)
                return GetStatisticsByTestAsync(SelectedTest, statisticsSortComparer);
            else
                return GetStatisticsByStudentAndTestAsync(SelectedStudent!, SelectedTest!, statisticsSortComparer);
        }

        private async Task<ImmutableSortedSet<TestResult>> GetAllStatisticsAsync(IComparer<TestResult> statisticsSortComparer)
        {
            using (TestingSystemTeacherContext context = new())
            {
                if (MinimumDate.HasValue)
                {
                    await context.TestResults
                                 .Where(testResult => testResult.CompletionDate > MinimumDate)
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();
                }
                else
                {
                    await context.TestResults
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();

                }

                return context.TestResults.ToImmutableSortedSet(statisticsSortComparer);
            }
        }

        private async Task<ImmutableSortedSet<TestResult>> GetStatisticsByStudentAsync(Models.Student student, IComparer<TestResult> statisticsSortComparer)
        {
            using (TestingSystemTeacherContext context = new())
            {
                if (MinimumDate.HasValue)
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Student.Id == student.Id && testResult.CompletionDate > MinimumDate)
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

                return context.TestResults.ToImmutableSortedSet(statisticsSortComparer);
            }
        }

        private async Task<ImmutableSortedSet<TestResult>> GetStatisticsByTestAsync(Test test, IComparer<TestResult> statisticsSortComparer)
        {
            using (TestingSystemTeacherContext context = new())
            {
                if (MinimumDate.HasValue)
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Test.Id == test.Id && testResult.CompletionDate > MinimumDate)
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();
                }
                else
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Test.Id == test.Id)
                                 .Include(testResult => testResult.Student)
                                 .Include(testResult => testResult.Test)
                                 .LoadAsync();
                }

                return context.TestResults.ToImmutableSortedSet(statisticsSortComparer);
            }
        }

        private async Task<ImmutableSortedSet<TestResult>> GetStatisticsByStudentAndTestAsync(Models.Student student, Test test, IComparer<TestResult> statisticsSortComparer)
        {
            using (TestingSystemTeacherContext context = new())
            {
                if (MinimumDate.HasValue)
                {
                    await context.TestResults
                                 .Where(testResult => testResult.Student.Id == student.Id
                                                       && testResult.Test.Id == test.Id && testResult.CompletionDate > MinimumDate)
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

                return context.TestResults.ToImmutableSortedSet(statisticsSortComparer);
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