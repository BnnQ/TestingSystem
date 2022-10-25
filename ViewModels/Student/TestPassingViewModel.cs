using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MvvmBaseViewModels.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        public ImmutableSortedSet<Question> Questions { get; init; } = null!;
        private IEnumerator<Question> questionsEnumerator = null!;

        private DateTime testStartTime;
        private DateTime testCompletionTime;
        private List<TimeSpan> questionAnswerTimes = null!;
        private ushort correctAnswersCounter;
        #endregion

        #region Current question
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
        public bool DoesCurrentQuestionOnlyHaveOneCorrectAnswer => CurrentQuestion.AnswerOptions.SingleOrDefault(answerOption => answerOption.IsCorrect) is not null;
        private DateTime currentQuestionStartTime;
        private DateTime currentQuestionCompletionTime;

        private List<AnswerOption> selectedAnswerOptions = null!;
        #endregion

        public TestPassingViewModel(INavigationManager navigationManager, Test test, Models.Student student) : base(navigationManager)
        {
            try
            {
                using (TestingSystemStudentContext context = new())
                {
                    Test? testEntity = context.Find<Test>(test.Id);
                    if (testEntity is null)
                        throw new NullReferenceException("Test entity missing from the database (most likely, a problem on the DB side)");

                    EntityEntry<Test> testEntry = context.Entry(testEntity);

                    testEntry.Collection(test => test.Questions).Load();
                    foreach (Question question in testEntity.Questions)
                        context.Entry(question).Collection(question => question.AnswerOptions).Load();

                    this.test = testEntity;
                    Questions = this.test.Questions.ToImmutableSortedSet(new QuestionBySerialNumberComparer());
                    questionsEnumerator = Questions.GetEnumerator();
                    questionAnswerTimes = new(Questions.Count);
                    selectedAnswerOptions = new();

                    testStartTime = DateTime.Now;
                    MoveToNextQuestion();
                }
            }
            catch (Exception exception)
            {
                OccurCriticalErrorMessage(exception);
                return;
            }

            this.student = student;
        }

        #region Commands
        private RelayCommand<AnswerOption> chooseAnswerOptionCommand = null!;
        public RelayCommand<AnswerOption> ChooseAnswerOptionCommand
        {
            get => chooseAnswerOptionCommand ??= new((answerOption) =>
            {
                if (DoesCurrentQuestionOnlyHaveOneCorrectAnswer)
                {
                    if (selectedAnswerOptions.Any())
                        selectedAnswerOptions.RemoveAt(0);
                    selectedAnswerOptions.Add(answerOption!);
                }
                else
                {
                    if (selectedAnswerOptions.Contains(answerOption!))
                        selectedAnswerOptions.Remove(answerOption!);
                    else
                        selectedAnswerOptions.Add(answerOption!);
                }
            }, answerOption => answerOption is not null);
        }

        private void MoveToNextQuestion()
        {
            if (CurrentQuestion is not null)
            {
                if (selectedAnswerOptions.SequenceEqual(CurrentQuestion.AnswerOptions))
                    correctAnswersCounter++;
            }

            if (questionsEnumerator.MoveNext())
            {
                currentQuestionCompletionTime = DateTime.Now;
                if (CurrentQuestion is not null)
                {
                    questionAnswerTimes.Add(currentQuestionCompletionTime - currentQuestionStartTime);
                    selectedAnswerOptions.Clear();
                }

                CurrentQuestion = questionsEnumerator.Current;
                currentQuestionStartTime = DateTime.Now;
            }
            else
            {
                testCompletionTime = DateTime.Now;

                ushort score = (ushort) (correctAnswersCounter * (test.MaximumPoints / test.NumberOfQuestions));
                ushort numberOfIncorrectAnswers = (ushort) (test.NumberOfQuestions - correctAnswersCounter);
                TimeSpan averageAnswerTime = TimeSpan.FromSeconds( questionAnswerTimes.Average(timeSpan => timeSpan.TotalSeconds));

                TestResults testResults = new(test, score, correctAnswersCounter, numberOfIncorrectAnswers,
                    testCompletionTime - testStartTime, averageAnswerTime);
                MoveToTestResults(new TestCompletedNavigationArgs(lastViewModelNavigatedFrom, testResults));
            }
        }

        private RelayCommand confirmSelectedAnswerOptionsCommand = null!;
        public RelayCommand ConfirmSelectedAnswerOptionsCommand
        {
            get => confirmSelectedAnswerOptionsCommand ??= new(MoveToNextQuestion, () => selectedAnswerOptions.Any());
        }

        private void MoveToTestResults(TestCompletedNavigationArgs navigationArgs)
        {
            navigationManager.Navigate(NavigationKeys.TestResults, navigationArgs);
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