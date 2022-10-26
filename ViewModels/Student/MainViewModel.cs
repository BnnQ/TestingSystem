using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore;
using MvvmBaseViewModels.Common;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Student;

namespace TestingSystem.ViewModels.Student
{
    public class MainViewModel : ViewModelBase
    {
        private Models.Student student = null!;

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

        public BackgroundWorker CategoriesUpdaterFromDatabaseBackgroundWorker { get; init; } = new();

        public MainViewModel(Models.Student student)
        {
            _ = InitialLoadAsync(student);
            SetupBackgroundWorkers();
            UpdateCategoriesFromDatabaseAsyncCommand.Execute(null);
        }

        private void SetupBackgroundWorkers()
        {
            CategoriesUpdaterFromDatabaseBackgroundWorker.MinimumWorkExecutionTime = 500;
            CategoriesUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateCategoriesFromDatabaseAsync();
            CategoriesUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () => CommandManager.InvalidateRequerySuggested();
        }

        private async Task InitialLoadAsync(Models.Student student)
        {
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

        private RelayCommand<Test> openTestCommand = null!;
        public RelayCommand<Test> OpenTestCommand
        {
            get => openTestCommand ??= new((test) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
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