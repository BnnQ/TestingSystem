using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MvvmBaseViewModels.Common;
using NeoSmart.AsyncLock;
using System.Linq;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.Views.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class CategoryInfoViewModel : ViewModelBase
    {
        private readonly TestingSystemTeacherContext databaseContext;
        private readonly AsyncLock databaseContextLocker;

        private Category category = null!;
        public Category Category
        {
            get => category;
            set
            {
                if (category != value)
                {
                    category = value;
                    OnPropertyChanged(nameof(Category));
                    OnPropertyChanged(nameof(NumberOfTests));
                }
            }
        }

        public int NumberOfTests => Category.Tests.Count;

        private readonly Models.Teacher teacher;


        public CategoryInfoViewModel(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker, 
            Category category, Models.Teacher teacher)
        {
            this.databaseContext = databaseContext;
            this.databaseContextLocker = databaseContextLocker;
            Category = category;
            this.teacher = teacher;
        }

        #region Commands
        private AsyncRelayCommand editCategoryAsyncCommand = null!;
        public AsyncRelayCommand EditCategoryAsyncCommand
        {
            get => editCategoryAsyncCommand ??= new(async () =>
            {
                Category? categoryInDatabase = default;
                using (await databaseContextLocker.LockAsync())
                {
                    await databaseContext.Categories
                    .Include(category => category.Tests)
                    .LoadAsync();

                    categoryInDatabase = await databaseContext.FindAsync<Category>(Category.Id);
                }

                if (categoryInDatabase is not null)
                {
                    bool? editViewDialogResult = default;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CategoryEditView categoryEditView = new(categoryInDatabase);
                        editViewDialogResult = categoryEditView.ShowDialog();
                    });

                    if (editViewDialogResult == true)
                    {
                        using (await databaseContextLocker.LockAsync())
                            await databaseContext.SaveChangesAsync();
                    }
                }

            }, 
            () =>
            {
                return Category is not null &&
                       (Category.Tests.Count <= 0 ||
                       Category.Tests.Any(test => test.OwnerTeachers.Any(teacher => teacher.Id == this.teacher.Id)));
            });
        }

        private AsyncRelayCommand removeCategoryAsyncCommand = null!;
        public AsyncRelayCommand RemoveCategoryAsyncCommand
        {
            get => removeCategoryAsyncCommand ??= new(async () =>
            {
                using (await databaseContextLocker.LockAsync())
                {
                    Category? categoryToBeRemoved = await databaseContext.FindAsync<Category>(Category.Id);
                    if (categoryToBeRemoved is not null)
                    {
                        databaseContext.Categories.Remove(categoryToBeRemoved);
                        await databaseContext.SaveChangesAsync();

                        Close();
                    }
                }
            }, 
            () =>
            {
                return Category is not null &&
                       (Category.Tests.Count <= 0 ||
                       Category.Tests.All(test => test.OwnerTeachers.Any(teacher => teacher.Id == this.teacher.Id)));
            });
        }
        #endregion

    }
}