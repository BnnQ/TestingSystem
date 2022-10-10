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
using MvvmBaseViewModelsLibrary.Enumerables;
using HappyStudio.Mvvm.Input.Wpf;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Constants.Authorization;
using MvvmBaseViewModels.Navigation;
using BackgroundWorkerLibrary;
using NeoSmart.AsyncLock;

namespace TestingSystem.ViewModels.Authorization
{
    public class AuthenticationViewModel : ValidatableNavigationViewModelBase
    {
        private readonly static ScryptEncoder encoder = new();

        private readonly AsyncLock databaseContextLocker;
        private readonly TestingSystemAuthorizationContext databaseContext;
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


        public AuthenticationViewModel(INavigationManager navigationManager, TestingSystemAuthorizationContext databaseContext,
            AsyncLock databaseContextLocker) 
            : base(navigationManager, true, ValidationState.Disabled)
        {
            this.databaseContext = databaseContext;
            this.databaseContextLocker = databaseContextLocker;

            SetupBackgroundWorkers();
            SetupValidator();

            _ = initialDatabaseLoadBackgroundWorker.RunWorkerAsync();
        }

        private void SetupBackgroundWorkers()
        {
            AuthenticationBackgroundWorker.DoWork = async () =>
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
                await AuthenticateAsync();
            };
            AuthenticationBackgroundWorker.OnWorkCompleted = () =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            };

            initialDatabaseLoadBackgroundWorker.DoWork = async () =>
            {
                using (await databaseContextLocker.LockAsync())
                {
                    await databaseContext.Teachers.LoadAsync();
                    await databaseContext.Students.LoadAsync();
                }
            };

        }

        private async Task AuthenticateAsync()
        {
            if (!await IsUsernameValidAsync())
                return;

            Models.Teacher? foundTeacher =
                teachers
                .AsParallel()
                .FirstOrDefault(teacher => encoder.Compare(Username, teacher.EncryptedName));

            if (foundTeacher is not null)
            {
                User = foundTeacher;
                if (!await IsPasswordValidAsync())
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Views.Teacher.MainView teacherView = new(foundTeacher);
                    Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Arrow);
                    teacherView.Show();

                    Close();
                });
               
            }

            Models.Student? foundStudent =
                students
                .AsParallel()
                .FirstOrDefault(student => encoder.Compare(Username, student.EncryptedName));

            if (foundStudent is not null)
            {
                User = foundStudent;
                if (!await IsPasswordValidAsync())
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Views.Student.MainView studentView = new(foundStudent);
                    Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Arrow);
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

            using (await databaseContextLocker.LockAsync())
            {
                await databaseContext.Teachers.LoadAsync();
                teachers = await databaseContext.Teachers.ToListAsync();
                if (teachers.AsParallel().Any(teacher => encoder.Compare(username, teacher.EncryptedName)))
                    return true;
            }
            
            using (await databaseContextLocker.LockAsync())
            {
                await databaseContext.Students.LoadAsync();
                students = await databaseContext.Students.ToListAsync();
                if (students.AsParallel().Any(student => encoder.Compare(username, student.EncryptedName)))
                    return true;
            }

            return false;
        }

        private bool IsUserPassedAuthentication(string password)
        {
            if (encoder.Compare(Username, User?.EncryptedName) &&
                encoder.Compare(password, User?.EncryptedPassword))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

    }
}