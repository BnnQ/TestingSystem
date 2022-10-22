﻿using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common.Validatable;
using MvvmBaseViewModels.Enums;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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


        private readonly bool doesCategoryExistInDatabase;
        private readonly TestingSystemTeacherContext context;

        public CategoryEditViewModel(Category category)
        {
            context = new TestingSystemTeacherContext();
            Category? categoryEntity = context.Find<Category>(category.Id);

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
        private AsyncRelayCommand confirmAsyncCommand = null!;
        public AsyncRelayCommand ConfirmAsyncCommand
        {
            get => confirmAsyncCommand ??= new(async () =>
            {
                if (!await IsNameValidAsync())
                    return;

                if (!doesCategoryExistInDatabase)
                    await context.Categories.AddAsync(Category);

                await context.SaveChangesAsync();
                Close(true);
            });
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion

        #region Disposing
        public override void Close(bool? dialogResult = null)
        {
            Dispose(true);
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