using Egor92.MvvmNavigation.Abstractions;
using Microsoft.EntityFrameworkCore;
using MvvmBaseViewModels.Navigation.Validatable;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using Scrypt;
using MvvmBaseViewModels.Enums;
using HappyStudio.Mvvm.Input.Wpf;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Constants.Authorization;
using MvvmBaseViewModels.Navigation;
using BackgroundWorkerLibrary;
using Z.Linq;
using System;
using WpfExceptionMessageBox;

namespace TestingSystem.ViewModels.Authorization
{
    public class AuthenticationViewModel : ValidatableNavigationViewModelBase
    {
        private readonly static ScryptEncoder encoder = new();

        private IList<Models.Teacher> teachers = null!;
        private IList<Models.Student> students = null!;

        private string username = null!;
        public string Username
        {
            get => username;
            set
            {
                string lowercaseValue = value.ToLowerInvariant();
                if (username != lowercaseValue)
                {
                    username = lowercaseValue;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        private string password = null!;
        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private User? user;
        public User? User
        {
            get => user;
            set
            {
                if (user != value)
                {
                    user = value;
                    OnPropertyChanged(nameof(User));
                }
            }
        }

        private ValidationState usernameValidationState = ValidationState.Disabled;
        private ValidationState passwordValidationState = ValidationState.Disabled;

        //Performs user authentication in background, including pulling up the actual data from the database
        //and checking whether the user entered the data for authentication correctly
        public BackgroundWorker AuthenticationBackgroundWorker { get; init; } = new();
        //Performs initial loading of data from the database in the background when the application starts
        private readonly BackgroundWorker initialDatabaseLoadBackgroundWorker = new();


        public AuthenticationViewModel(INavigationManager navigationManager) 
            : base(navigationManager, true, ValidationState.Disabled)
        {
            SetupBackgroundWorkers();
            SetupValidator();

            _ = initialDatabaseLoadBackgroundWorker.RunWorkerAsync();
        }


        #region Updating data from database
        private async Task UpdateTeachersFromDatabaseAsync()
        {
            try
            {
                using (TestingSystemAuthorizationContext context = new())
                {
                    await context.Teachers.LoadAsync();
                    teachers = await context.Teachers.Local.ToListAsync();
                }
            }
            catch (Exception exception)
            {
                OccurCriticalErrorMessage(exception);
            }
        }

        private async Task UpdateStudentsFromDatabaseAsync()
        {
            try
            {
                using (TestingSystemAuthorizationContext context = new())
                {
                    await context.Students.LoadAsync();
                    students = await context.Students.Local.ToListAsync();
                }
            }
            catch (Exception exception)
            {
                OccurCriticalErrorMessage(exception);
            }
        }

        private async Task UpdateDataFromDatabaseAsync()
        {
            await UpdateTeachersFromDatabaseAsync();
            await UpdateStudentsFromDatabaseAsync();
        }
        #endregion

        private void SetupBackgroundWorkers()
        {
            AuthenticationBackgroundWorker.DoWork = async () => await AuthenticateAsync();

            initialDatabaseLoadBackgroundWorker.DoWork = async () => await UpdateDataFromDatabaseAsync();
        }

        private async Task AuthenticateAsync()
        {
            if (!await IsUsernameValidAsync())
                return;

            Models.Teacher? foundTeacher =
                teachers
                .AsParallel()
                .FirstOrDefault(teacher => Username.Equals(teacher.Name));

            if (foundTeacher is not null)
            {
                User = foundTeacher;
                if (!await IsPasswordValidAsync())
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Views.Teacher.MainView teacherView = new(foundTeacher);
                    teacherView.Show();

                    Close();
                });
               
            }

            Models.Student? foundStudent =
                students
                .AsParallel()
                .FirstOrDefault(student => Username.Equals(student.Name));

            if (foundStudent is not null)
            {
                User = foundStudent;
                if (!await IsPasswordValidAsync())
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Views.Student.MainView studentView = new(foundStudent);
                    studentView.Show();
                    
                    Close();
                });

            }

        }

        #region Commands
        private AsyncRelayCommand authenticateAsyncCommand = null!;
        public AsyncRelayCommand AuthenticateAsyncCommand
        {
            get => authenticateAsyncCommand ??= new(async () =>
            {
                if (!AuthenticationBackgroundWorker.IsBusy)
                        await AuthenticationBackgroundWorker.RunWorkerAsync();
            });
        }

        private RelayCommand switchToRegistrationNavigationCommand = null!;
        public RelayCommand SwitchToRegistrationNavigationCommand
        {
            get => switchToRegistrationNavigationCommand ??= new(() =>
            {
                navigationManager.Navigate(NavigationKeys.Registration, new NavigationArgs(this));
            });
        }
        #endregion

        #region Validation setup
        private async Task<bool> IsUsernameValidAsync()
        {
            usernameValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            usernameValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsPasswordValidAsync()
        {
            passwordValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            passwordValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        protected override void SetupValidator()
        {
            ValidationBuilder<AuthenticationViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Username)
                .NotEmpty()
                .When(viewModel => viewModel.usernameValidationState == ValidationState.Enabled)
                .WithMessage("Поле не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Username)
                .Must(IsUsernameExistsAsync)
                .When(viewModel => viewModel.usernameValidationState == ValidationState.Enabled)
                .WithMessage("Пользователя с таким именем не существует");

            builder.RuleFor(viewModel => viewModel.Password)
                .NotEmpty()
                .When(viewModel => viewModel.passwordValidationState == ValidationState.Enabled)
                .WithMessage("Поле не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Password)
                .Must(IsUserPassedAuthentication)
                .When(viewModel => viewModel.passwordValidationState == ValidationState.Enabled)
                .WithMessage("Неверный пароль");

            Validator = builder.Build(this);
        }

        private async Task<bool> IsUsernameExistsAsync(string username)
        {
            username = username.ToLower();

            await UpdateTeachersFromDatabaseAsync();
            if (teachers.AsParallel().Any(teacher => username.Equals(teacher.Name)))
                return true;

            await UpdateStudentsFromDatabaseAsync();
            if (students.AsParallel().Any(student => username.Equals(student.Name)))
                return true;

            return false;
        }

        private bool IsUserPassedAuthentication(string password)
        {
            return Username.Equals(User?.Name) && encoder.Compare(password, User?.HashedPassword);
        }
        #endregion

    }
}