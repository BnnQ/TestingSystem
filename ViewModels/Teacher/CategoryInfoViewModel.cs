using BackgroundWorkerLibrary;
using CommunityToolkit.Mvvm.Input;
using HelperDialogs.Views;
using Meziantou.Framework.WPF.Extensions;
using MvvmBaseViewModels.Common;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class CategoryInfoViewModel : ViewModelBase
    {
        private void OnCategoryTestsChanged(object? _, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove)
                OnPropertyChanged(nameof(NumberOfTests));
        }
        private Category category = null!;
        public Category Category
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

        private readonly Models.Teacher teacher = null!;
        public BackgroundWorker CategoryUpdaterFromDatabaseBackgroundWorker { get; init; } = new();

        public CategoryInfoViewModel(Category category, Models.Teacher teacher)
        {
            Category = category;
            this.teacher = teacher;

            SetupBackgroundWorkers();

            _ = UpdateCategoryFromDatabaseAsyncCommand.ExecuteAsync(null);
        }

        private void SetupBackgroundWorkers()
        {
            CategoryUpdaterFromDatabaseBackgroundWorker.OnWorkStarting = () =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                CommandManager.InvalidateRequerySuggested();
            };
            CategoryUpdaterFromDatabaseBackgroundWorker.DoWork = async () => await UpdateCategoryFromDatabaseAsync();
            CategoryUpdaterFromDatabaseBackgroundWorker.OnWorkCompleted = () =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                CommandManager.InvalidateRequerySuggested();
            };
        }
        
        #region Commands
        private async Task UpdateCategoryFromDatabaseAsync()
        {
                try
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
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }
        }

        private AsyncRelayCommand updateCategoryFromDatabaseAsyncCommand = null!;
        public AsyncRelayCommand UpdateCategoryFromDatabaseAsyncCommand
        {
            get => updateCategoryFromDatabaseAsyncCommand ??= new(async () =>
            {
                if (!CategoryUpdaterFromDatabaseBackgroundWorker.IsBusy)
                    await CategoryUpdaterFromDatabaseBackgroundWorker.RunWorkerAsync();
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
            },
            () => Category is not null && !CategoryUpdaterFromDatabaseBackgroundWorker.IsBusy && 
            (AreTestsEmpty() || DoesTeacherOwnAtLeastOneTest()));
        }

        private bool isRemoveLocked = false;
        private AsyncRelayCommand removeCategoryAsyncCommand = null!;
        public AsyncRelayCommand RemoveCategoryAsyncCommand
        {
            get => removeCategoryAsyncCommand ??= new(async () =>
            {
                ConfirmationDialogView confirmationDialog = new(
                    warningMessage: "Вы уверены что хотите удалить категорию?",
                    descriptionMessage: "Это действие нельзя будет отменить.",
                    cancelText: "Оставить категорию",
                    confirmText: "Удалить категорию");
                
                bool? confirmationDialogResult = default;
                Application.Current?.Dispatcher.Invoke(() => confirmationDialogResult = confirmationDialog.ShowDialog());
                if (confirmationDialogResult == true)
                {
                    isRemoveLocked = true;
                    try
                    {
                        using (TestingSystemTeacherContext context = new())
                        {
                            context.Categories.Remove(Category!);
                            await context.SaveChangesAsync();

                            Close();
                        }
                    }
                    catch (Exception exception)
                    {
                        OccurCriticalErrorMessage(exception);
                        return;
                    }
                }

            }, 
            () => !isRemoveLocked && Category is not null && !CategoryUpdaterFromDatabaseBackgroundWorker.IsBusy &&
            (AreTestsEmpty() || DoesTeacherOwnAllTests()));
        }
        #endregion

    }
}