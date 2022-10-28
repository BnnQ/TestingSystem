using BackgroundWorkerLibrary;
using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
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

namespace TestingSystem.ViewModels.Student
{
    public class TestPassingViewModel : NavigationViewModelBase
    {
        private readonly Test test = null!;
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
                        _ = CurrentQuestion.AnswerOptions.Single(answerOption => answerOption.IsCorrect);

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

        public BackgroundWorker InitialLoaderBackgroundWorker { get; init; } = new();

        public TestPassingViewModel(INavigationManager navigationManager, Test test, Models.Student student) : base(navigationManager)
        {
            this.test = test;
            this.student = student;
            
            SetupBackgroundWorkers();
            _ = InitialLoaderBackgroundWorker.RunWorkerAsync();
        }

        private void SetupBackgroundWorkers()
        {
            InitialLoaderBackgroundWorker.DoWork = () =>
            {
                return Task.Run(() =>
                {
                    Questions = test.Questions.ToImmutableSortedSet(new QuestionBySerialNumberComparer());
                    questionsEnumerator = Questions.GetEnumerator();
                    questionAnswerTimes = new(Questions.Count);
                    SelectedAnswerOptions = new List<AnswerOption>();

                    testStartTime = DateTime.Now;
                    if (test.NumberOfSecondsToComplete.HasValue)
                        _ = testTimerBackgroundWorker.RunWorkerAsync(test.NumberOfSecondsToComplete.Value);

                    lock (currentQuestionLocker)
                        MoveToNextQuestion();
                });
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

                TestResult testResults = FinishTest();
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
                    bool hasCorrectAnswerOptions = false;
                    foreach (AnswerOption selectedAnswerOption in SelectedAnswerOptions)
                    {
                        if (correctAnswerOptions.Contains(selectedAnswerOption))
                        {
                            scoreAccumulator += oneAnswerOptionCost;
                            hasCorrectAnswerOptions = true;
                        }
                    }
                    
                    if (hasCorrectAnswerOptions)
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
                TestResult testResults = FinishTest();
                MoveToTestResults(new TestCompletedNavigationArgs(lastViewModelNavigatedFrom, testResults));
            }
        }

        private TestResult FinishTest()
        {
            testCompletionTime = DateTime.Now;

            ushort score = Convert.ToUInt16(scoreAccumulator);
            ushort numberOfIncorrectAnswers = (ushort) (test.NumberOfQuestions - correctAnswersCounter);
            TimeSpan averageAnswerTime = TimeSpan.FromSeconds(questionAnswerTimes.Average(timeSpan => timeSpan.TotalSeconds));

             return new TestResult(test, student, score, correctAnswersCounter, numberOfIncorrectAnswers,
                testCompletionTime.Value - testStartTime!.Value, averageAnswerTime, DateTime.Now);
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