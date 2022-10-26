using BackgroundWorkerLibrary;
using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModels.Enums;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;

namespace TestingSystem.ViewModels.Teacher
{
    public class CategoryEditViewModel : ValidatableViewModelBase
    {
        private void OnCategoryChanged(object? _, PropertyChangedEventArgs args) => OnPropertyChanged(args.PropertyName);
        private Category category = null!;
        public Category Category
        {
            get => category;
            set
            {
                if (category != value)
                {
                    if (category is not null)
                        category.PropertyChanged += OnCategoryChanged;

                    category = value;
                    OnPropertyChanged(nameof(Category));

                    if (category is not null)
                        category.PropertyChanged += OnCategoryChanged;
                }
            }
        }

        public string Name
        {
            get => Category.Name;
            set => Category.Name = value;
        }


        private bool doesCategoryExistInDatabase;
        private readonly TestingSystemTeacherContext context = null!;
        public BackgroundWorker<Category> InitialLoaderBackgroundWorker { get; init; } = new();

        public CategoryEditViewModel(Category category)
        {
            context = new TestingSystemTeacherContext();
            
            SetupBackgroundWorkers();
            _ = InitialLoaderBackgroundWorker.RunWorkerAsync(category);
        }

        public void SetupBackgroundWorkers()
        {
            InitialLoaderBackgroundWorker.OnWorkStarting = () => Mouse.OverrideCursor = Cursors.Wait;
            InitialLoaderBackgroundWorker.DoWork = async (parameters) =>
            {
                if (parameters?.Length < 1)
                    return;

                Category category = parameters!.First();
                Category? categoryEntity;
                try
                {
                    categoryEntity = await context.FindAsync<Category>(category.Id);
                }
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }

                if (categoryEntity is not null)
                {
                    doesCategoryExistInDatabase = true;
                    Category = categoryEntity;
                }
                else
                {
                    doesCategoryExistInDatabase = false;
                    Category = new(category.Name);
                }

                SetupValidator();
            };
            InitialLoaderBackgroundWorker.OnWorkStarting = () => Mouse.OverrideCursor = Cursors.Arrow;
        }

        #region Validation setup
        private ValidationState nameValidationState = ValidationState.Disabled;
        protected override void SetupValidator()
        {
            ValidationBuilder<CategoryEditViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Name)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .When(viewModel => viewModel.nameValidationState == ValidationState.Enabled)
                .WithMessage("Название не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Name)
                .MaxLength(128)
                .When(viewModel => viewModel.nameValidationState == ValidationState.Enabled)
                .WithMessage("Название не может быть длиннее 128 символов");

            Validator = builder.Build(this);
        }

        private async Task<bool> IsNameValidAsync()
        {
            nameValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            nameValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }
        #endregion

        #region Commands
        private bool isConfirmLocked = false;
        private AsyncRelayCommand confirmAsyncCommand = null!;
        public AsyncRelayCommand ConfirmAsyncCommand
        {
            get => confirmAsyncCommand ??= new(async () =>
            {
                isConfirmLocked = true;
                if (!await IsNameValidAsync())
                {
                    isConfirmLocked = false;
                    return;
                }

                isConfirmLocked = true;
                try
                {
                    if (!doesCategoryExistInDatabase)
                        await context.Categories.AddAsync(Category);

                    await context.SaveChangesAsync();
                    Close(true);
                }
                catch (Exception exception)
                {
                    OccurCriticalErrorMessage(exception);
                    return;
                }
            }, () => !isConfirmLocked);
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false), () => !isConfirmLocked);
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
        protected override void Dispose(bool needDisposing)
        {
            if (isDisposed)
                return;

            if (needDisposing)
            {
                context.Dispose();
            }

            isDisposed = true;
        }
        #endregion

    }
}