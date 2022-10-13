using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common;
using MvvmBaseViewModels.Common.Validatable;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using TestingSystem.Models;

namespace TestingSystem.ViewModels.Teacher
{
    public class CategoryEditViewModel : ValidatableViewModelBase
    {
        private readonly Category category;

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


        public CategoryEditViewModel(Category category)
        {
            this.category = category;

            Name = category.Name;
            SetupValidator();
        }

        protected override void SetupValidator()
        {
            ValidationBuilder<CategoryEditViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Name)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Название не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Name)
                .MaxLength(128)
                .WithMessage("Название не может быть длиннее 128 символов");

            Validator = builder.Build(this);
        }

        #region Commands
        private RelayCommand confirmCommand = null!;
        public RelayCommand ConfirmCommand
        {
            get => confirmCommand ??= new(() =>
            {
                category.Name = Name;
                Close(true);
            }, () => !string.IsNullOrWhiteSpace(Name) && Name.Length <= 128);
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion

    }
}