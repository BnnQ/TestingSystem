using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModelsLibrary.Enumerables;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System.Threading.Tasks;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;

namespace TestingSystem.ViewModels.Teacher
{
    public class CategoryEditViewModel : ValidatableViewModelBase
    {
        private Category category = null!;

        private string name = null!;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private readonly bool doesCategoryExistInDatabase;


        public CategoryEditViewModel(Category category)
        {
            Category? categoryEntity = default;
            using (TestingSystemTeacherContext context = new())
                categoryEntity = context.Find<Category>(category.Id);

            if (categoryEntity is not null)
            {
                doesCategoryExistInDatabase = true;
                this.category = categoryEntity;
            }
            else
            {
                doesCategoryExistInDatabase = false;
                this.category = category;
            }

            Name = this.category.Name;
            SetupValidator();
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
        private void SaveCategoryChangesLocally()
        {
            category.Name = Name;
        }
        private AsyncRelayCommand confirmAsyncCommand = null!;
        public AsyncRelayCommand ConfirmAsyncCommand
        {
            get => confirmAsyncCommand ??= new(async () =>
            {
                if (!await IsNameValidAsync())
                    return;

                if (doesCategoryExistInDatabase)
                {
                    using (TestingSystemTeacherContext context = new())
                    {
                        category = (await context.FindAsync<Category>(category.Id))!;
                        SaveCategoryChangesLocally();
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    SaveCategoryChangesLocally();
                }

                Close(true);
            });
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion

    }
}