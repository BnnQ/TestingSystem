using BackgroundWorkerLibrary;
using CommunityToolkit.Mvvm.Input;
using Meziantou.Framework.WPF.Extensions;
using MvvmBaseViewModels.Common;
using System.Collections.Specialized;
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
    public class CategoryInfoViewModel : ViewModelBase
    {
        private void OnCategoryTestsChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove)
                OnPropertyChanged(nameof(NumberOfTests));
        }
        private Category? category = null!;
        public Category? Category
        {
            get => category;
            set
            {
                if (category != value)
                {
                    if (category?.Tests is not null)
                        category.Tests.AsConcurrentObservableCollection().AsObservable.CollectionChanged -= OnCategoryTestsChanged;

                    category = value;
                    if (category?.Tests is not null)
                        category.Tests.AsConcurrentObservableCollection().AsObservable.CollectionChanged += OnCategoryTestsChanged;

                    OnPropertyChanged(nameof(Category));
                    OnPropertyChanged(nameof(NumberOfTests));
                }
            }
        }
        public int NumberOfTests => Category?.Tests.Count ?? 0;

        private readonly Models.Teacher teacher;
        private readonly BackgroundWorker categoryUpdaterFromDatabaseBackgroundWorker = new();

        public CategoryInfoViewModel(Category category, Models.Teacher teacher)
        {
            using (TestingSystemTeacherContext context = new())
            {
                Category? categoryEntity = context.Find<Category>(category.Id);
                if (categoryEntity is null)
                    OccurCriticalErrorMessage("Category entity missing from the database (most likely, a problem on the DB side)");
                else
                    Category = categoryEntity;
            }

            this.teacher = teacher;
            SetupBackgroundWorkers();

            _ = UpdateCategoryFromDatabaseAsyncCommand.ExecuteAsync(null);
        }

        private void SetupBackgroundWorkers()
        {
            categoryUpdaterFromDatabaseBackgroundWorker.OnWorkStarting = () => Mouse.OverrideCursor = Cursors.Wait;
            categoryUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateCategoryFromDatabaseAsync();
            categoryUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () => Mouse.OverrideCursor = Cursors.Arrow;
        }
        
        #region Commands
        private async Task UpdateCategoryFromDatabaseAsync()
        {
            if (Category is not null)
            {
                using (TestingSystemTeacherContext context = new())
                {
                    Category = (await context.FindAsync<Category>(Category.Id))!;
                    
                    await context.Entry(Category)
                        .Collection(category => category.Tests)
                        .LoadAsync();

                    foreach (Test test in Category.Tests)
                        await context.Entry(test).Collection(test => test.OwnerTeachers).LoadAsync();
                }
            }
        }

        private AsyncRelayCommand updateCategoryFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateCategoryFromDatabaseAsyncCommand
        {
            get => updateCategoryFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!categoryUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await categoryUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
            });
        }

        private bool AreTestsEmpty() => Category?.Tests.Count <= 0;
        private bool DoesTeacherOwnAtLeastOneTest()
        {
            if (Category is null)
            {
                return false;
            }
            else
            {
                return Category.Tests
                    .Any(test => test.OwnerTeachers.Any(teacher => teacher.Id == this.teacher.Id));
            }
        }
        private bool DoesTeacherOwnAllTests()
        {
            if (Category is null)
            {
                return false;
            }
            else
            {
                return Category.Tests
                    .All(test => test.OwnerTeachers.Any(teacher => teacher.Id == this.teacher.Id));
            }
        }

        private AsyncRelayCommand editCategoryAsyncCommand = null!;
        public AsyncRelayCommand EditCategoryAsyncCommand
        {
            get => editCategoryAsyncCommand ??= new(async () =>
            {
                bool? editViewDialogResult = default;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CategoryEditView categoryEditView = new(Category!);
                    editViewDialogResult = categoryEditView.ShowDialog();
                });

                if (editViewDialogResult == true)
                    await UpdateCategoryFromDatabaseAsyncCommand.ExecuteAsync(null);
            }, () => Category is not null && (AreTestsEmpty() || DoesTeacherOwnAtLeastOneTest()));
        }

        private AsyncRelayCommand removeCategoryAsyncCommand = null!;
        public AsyncRelayCommand RemoveCategoryAsyncCommand
        {
            get => removeCategoryAsyncCommand ??= new(async () =>
            {
                using (TestingSystemTeacherContext context = new())
                {
                    context.Categories.Remove(Category!);
                    await context.SaveChangesAsync();

                    Close();
                }
            }, () => Category is not null && (AreTestsEmpty() || DoesTeacherOwnAllTests()));
        }
        #endregion

    }
}