using BackgroundWorkerLibrary;
using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Constants.Student;
using TestingSystem.Helpers.Comparers;
using TestingSystem.Helpers.CustomNavigationArgs;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;

namespace TestingSystem.ViewModels.Student
{
    public class TestPassingViewModel : NavigationViewModelBase
    {
        private Test test = null!;
        private readonly Models.Student student = null!;

        #region Test
        public ImmutableSortedSet<Question> Questions { get; private set; } = null!;
        private IEnumerator<Question> questionsEnumerator = null!;

        private DateTime? testStartTime = null;
        private DateTime? testCompletionTime = null;
        private List<TimeSpan> questionAnswerTimes = null!;

        private ushort correctAnswersCounter;
        private double scoreAccumulator;

        private readonly BackgroundWorker<ushort> testTimerBackgroundWorker = new();
        private DateTime? testTimeLeft;
        public DateTime? TestTimeLeft
        {
            get => testTimeLeft;
            set
            {
                if (testTimeLeft != value)
                {
                    testTimeLeft = value;
                    OnPropertyChanged(nameof(TestTimeLeft));
                }
            }
        }
        #endregion

        #region Current question
        private readonly object currentQuestionLocker = new();
        private Question currentQuestion = null!;
        public Question CurrentQuestion
        {
            get => currentQuestion;
            set
            {
                if (currentQuestion != value)
                {
                    currentQuestion = value;
                    OnPropertyChanged(nameof(CurrentQuestion));
                    OnPropertyChanged(nameof(DoesCurrentQuestionOnlyHaveOneCorrectAnswer));
                }
            }
        }

        private ImmutableSortedSet<AnswerOption> currentQuestionAnswerOptions = null!;
        public ImmutableSortedSet<AnswerOption> CurrentQuestionAnswerOptions
        {
            get => currentQuestionAnswerOptions;
            set
            {
                if (currentQuestionAnswerOptions != value)
                {
                    currentQuestionAnswerOptions = value;
                    OnPropertyChanged(nameof(CurrentQuestionAnswerOptions));
                }
            }
        }

        public bool DoesCurrentQuestionOnlyHaveOneCorrectAnswer
        {
            get
            {
                try
                {
                    lock (currentQuestionLocker)
                        CurrentQuestion.AnswerOptions.Single(answerOption => answerOption.IsCorrect);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        private DateTime currentQuestionStartTime;
        private DateTime currentQuestionCompletionTime;

        private readonly BackgroundWorker<ushort> currentQuestionTimerBackgroundWorker = new();
        private DateTime? currentQuestionTimeLeft;
        public DateTime? CurrentQuestionTimeLeft
        {
            get => currentQuestionTimeLeft;
            set
            {
                if (currentQuestionTimeLeft != value)
                {
                    currentQuestionTimeLeft = value;
                    OnPropertyChanged(nameof(CurrentQuestionTimeLeft));
                }
            }
        }

        public List<AnswerOption> SelectedAnswerOptions { get; private set; } = null!;
        #endregion

        public BackgroundWorker<Test> InitialLoaderBackgroundWorker { get; init; } = new();

        public TestPassingViewModel(INavigationManager navigationManager, Test test, Models.Student student) : base(navigationManager)
        {
            this.student = student;
            
            SetupBackgroundWorkers();
            _ = InitialLoaderBackgroundWorker.RunWorkerAsync(test);
        }

        private void SetupBackgroundWorkers()
        {
            InitialLoaderBackgroundWorker.DoWork = async (parameters) =>
            {
                if (parameters?.Length < 1)
                    return;

                Test test = parameters!.First();
                try
                {
                    using (TestingSystemStudentContext context = new())
                    {
                        Test? testEntity = await context.FindAsync<Test>(test.Id);
                        if (testEntity is null)
                            throw new NullReferenceException("Test entity missing from the database (most likely, a problem on the DB side)");

                        EntityEntry<Test> testEntry = context.Entry(testEntity);

                        await testEntry.Collection(test => test.Questions).LoadAsync();
                        foreach (Question question in testEntity.Questions)
                            await context.Entry(question).Collection(question => question.AnswerOptions).LoadAsync();

                        this.test = testEntity;
                        Questions = this.test.Questions.ToImmutableSortedSet(new QuestionBySerialNumberComparer());
                        questionsEnumerator = Questions.GetEnumerator();
                        questionAnswerTimes = new(Questions.Count);
                        SelectedAnswerOptions = new List<AnswerOption>();

                        testStartTime = DateTime.Now;
                        if (test.NumberOfSecondsToComplete.HasValue)
                            _ = testTimerBackgroundWorker.RunWorkerAsync(test.NumberOfSecondsToComplete.Value);

                        lock (currentQuestionLocker)
                            MoveToNextQuestion();
                    }
                }
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }
            };

            testTimerBackgroundWorker.DoWork = async (parameters) =>
            {
                if (parameters?.Length < 1)
                    return;

                ushort numberOfSeconds = parameters!.First();
                TestTimeLeft = new(TimeSpan.TicksPerSecond * numberOfSeconds);
                while (numberOfSeconds != 0)
                {
                    await Task.Delay(1000);
                    numberOfSeconds--;

                    if (testCompletionTime.HasValue)
                        return;

                    TestTimeLeft = TestTimeLeft.Value.AddSeconds(-1);
                }

                TestResults testResults = FinishTest();
                MoveToTestResults(new TestCompletedNavigationArgs(lastViewModelNavigatedFrom, testResults));
            };

            currentQuestionTimerBackgroundWorker.DoWork = async (parameters) =>
            {
                Question initialQuestion = CurrentQuestion;
                if (parameters?.Length < 1 || initialQuestion is null)
                    return;

                ushort numberOfSeconds = parameters!.First();
                CurrentQuestionTimeLeft = new(TimeSpan.TicksPerSecond * numberOfSeconds);
                while (numberOfSeconds != 0)
                {
                    await Task.Delay(1000);
                    numberOfSeconds--;
                    
                    lock (currentQuestionLocker)
                    {
                        if (CurrentQuestion != initialQuestion)
                            return;
                    }

                    CurrentQuestionTimeLeft = CurrentQuestionTimeLeft.Value.AddSeconds(-1);
                }
                
                CurrentQuestionTimeLeft = null;
                lock (currentQuestionLocker)
                    MoveToNextQuestion();
            };
        }

        private void MoveToNextQuestion()
        {
            if (CurrentQuestion is not null)
            {
                IEnumerable<AnswerOption> correctAnswerOptions = currentQuestionAnswerOptions.Where(answerOption => answerOption.IsCorrect);
                if (SelectedAnswerOptions.SequenceEqual(correctAnswerOptions))
                {
                    scoreAccumulator += CurrentQuestion.PointsCost;
                    correctAnswersCounter++;
                }
                else if (test.IsAccountingForIncompleteAnswersEnabled)
                {
                    double oneAnswerOptionCost = CurrentQuestion.PointsCost / CurrentQuestion.NumberOfAnswerOptions;
                    foreach (AnswerOption selectedAnswerOption in SelectedAnswerOptions)
                    {
                        if (correctAnswerOptions.Contains(selectedAnswerOption))
                            scoreAccumulator += oneAnswerOptionCost;
                    }
                    correctAnswersCounter++;
                }
            }

            currentQuestionCompletionTime = DateTime.Now;
            if (CurrentQuestion is not null)
            {
                questionAnswerTimes.Add(currentQuestionCompletionTime - currentQuestionStartTime);
                SelectedAnswerOptions.Clear();
            }

            if (questionsEnumerator.MoveNext())
            {
                CurrentQuestion = questionsEnumerator.Current;
                var sortedAnswerOptions = CurrentQuestion.AnswerOptions.ToImmutableSortedSet(new AnswerOptionBySerialNumberComparer());
                CurrentQuestionAnswerOptions = sortedAnswerOptions;
                if (CurrentQuestion.NumberOfSecondsToAnswer.HasValue)
                    _ = currentQuestionTimerBackgroundWorker.RunWorkerAsync(CurrentQuestion.NumberOfSecondsToAnswer.Value);
                currentQuestionStartTime = DateTime.Now;
            }
            else
            {
                TestResults testResults = FinishTest();
                MoveToTestResults(new TestCompletedNavigationArgs(lastViewModelNavigatedFrom, testResults));
            }
        }

        private TestResults FinishTest()
        {
            testCompletionTime = DateTime.Now;

            ushort score = Convert.ToUInt16(scoreAccumulator);
            ushort numberOfIncorrectAnswers = (ushort) (test.NumberOfQuestions - correctAnswersCounter);
            TimeSpan averageAnswerTime = TimeSpan.FromSeconds(questionAnswerTimes.Average(timeSpan => timeSpan.TotalSeconds));

             return new TestResults(test, score, correctAnswersCounter, numberOfIncorrectAnswers,
                testCompletionTime.Value - testStartTime!.Value, averageAnswerTime);
        }
        private void MoveToTestResults(TestCompletedNavigationArgs navigationArgs)
        {
            navigationManager.Navigate(NavigationKeys.TestResults, navigationArgs);
        }

        #region Commands
        private bool isChooseAnswerOptionCommandLocked = false;
        private RelayCommand<AnswerOption> chooseAnswerOptionCommand = null!;
        public RelayCommand<AnswerOption> ChooseAnswerOptionCommand
        {
            get => chooseAnswerOptionCommand ??= new((answerOption) =>
            {
                isChooseAnswerOptionCommandLocked = true;
                if (DoesCurrentQuestionOnlyHaveOneCorrectAnswer)
                {
                    if (SelectedAnswerOptions.Any())
                    {
                        if (SelectedAnswerOptions.First() == answerOption)
                            SelectedAnswerOptions.Clear();
                    }
                    else
                    {
                        SelectedAnswerOptions.Add(answerOption!);
                    }
                }
                else
                {
                    if (SelectedAnswerOptions.Contains(answerOption!))
                        SelectedAnswerOptions.Remove(answerOption!);
                    else
                        SelectedAnswerOptions.Add(answerOption!);
                }
                isChooseAnswerOptionCommandLocked = false;
            }, answerOption => answerOption is not null && !isChooseAnswerOptionCommandLocked);
        }

        private RelayCommand confirmSelectedAnswerOptionsCommand = null!;
        public RelayCommand ConfirmSelectedAnswerOptionsCommand
        {
            get => confirmSelectedAnswerOptionsCommand ??= new(MoveToNextQuestion, () => SelectedAnswerOptions.Any());
        }
        #endregion

        #region Disposing
        public override void Close(bool? dialogResult = null)
        {
            Dispose();
            base.Close(dialogResult);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        private bool isDisposed = false;
        protected virtual void Dispose(bool needDisposing)
        {
            if (isDisposed)
                return;

            if (needDisposing)
            {
                questionsEnumerator?.Dispose();
            }

            isDisposed = true;
        }
        #endregion
    }
}