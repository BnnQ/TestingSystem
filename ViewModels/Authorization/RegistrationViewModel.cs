using BackgroundWorkerLibrary;
using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
using Microsoft.EntityFrameworkCore;
using MvvmBaseViewModels.Navigation;
using MvvmBaseViewModels.Navigation.Validatable;
using MvvmBaseViewModelsLibrary.Enumerables;
using ReactiveValidation;
using ReactiveValidation.Extensions;
using Scrypt;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Constants.Authorization;
using TestingSystem.Models.Contexts;
using Z.Linq;

namespace TestingSystem.ViewModels.Authorization
{
    public class RegistrationViewModel : ValidatableNavigationViewModelBase
    {
        private readonly static ScryptEncoder encoder = new();

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

        private string fullName = null!;
        public string FullName
        {
            get => fullName;
            set
            {
                if (fullName != value)
                {
                    fullName = value;
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private bool isStudent = true;
        public bool IsStudent
        {
            get => isStudent;
            set
            {
                if (isStudent != value)
                {
                    isStudent = value;
                    OnPropertyChanged(nameof(IsStudent));
                }
            }
        }

        private IList<Models.Teacher> teachers = null!;
        private IList<Models.Student> students = null!;

        //Performs registration of a user in the background,
        //including verification that the data matches the requirements and such a user is not yet registered
        public BackgroundWorker RegistrationBackgroundWorker { get; init; } = new();
        //Performs initial loading of data from the database in the background when the application starts
        private readonly BackgroundWorker initialDatabaseLoadBackgroundWorker = new();

        public RegistrationViewModel(INavigationManager navigationManager) : base(navigationManager)
        {
            SetupBackgroundWorkers();
            SetupValidator();

            _ = initialDatabaseLoadBackgroundWorker.RunWorkerAsync();
        }

        private void SetupBackgroundWorkers()
        {
            RegistrationBackgroundWorker.DoWork = async () =>
            {
                Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
                await RegisterAsync();
            };
            RegistrationBackgroundWorker.OnWorkCompleted = () =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            };

            initialDatabaseLoadBackgroundWorker.DoWork = async () =>
            {
                await UpdateStudentsFromDatabaseAsync();
            };
        }


        #region Updating data from database
        private async Task UpdateTeachersFromDatabaseAsync()
        {
            using (TestingSystemAuthorizationContext context = new())
            {
                await context.Teachers.LoadAsync();
                teachers = await context.Teachers.Local.ToListAsync();
            }
        }

        private async Task UpdateStudentsFromDatabaseAsync()
        {
            using (TestingSystemAuthorizationContext context = new())
            {
                await context.Students.LoadAsync();
                students = await context.Students.Local.ToListAsync();
            }
        }
        #endregion

        private async Task RegisterAsync()
        {
            if (!await IsUsernameValidAsync() || !await IsPasswordValidAsync() || !await IsFullNameValidAsync())
                return;

            string hashedUsername = encoder.Encode(Username);
            string hashedPassword = encoder.Encode(Password);

            if (IsStudent)
            {
                Models.Student registeredStudent = new(hashedUsername, hashedPassword, FullName);
                using (TestingSystemAuthorizationContext context = new())
                {
                    await context.Students.AddAsync(registeredStudent);
                    await context.SaveChangesAsync();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Views.Student.MainView studentMainView = new(registeredStudent);
                    Mouse.OverrideCursor = Cursors.Arrow;
                    studentMainView.Show();

                    Close();
                });
            }
            else
            {
                Models.Teacher registeredTeacher = new(hashedUsername, hashedPassword, FullName);
                using (TestingSystemAuthorizationContext context = new())
                {
                    await context.Teachers.AddAsync(registeredTeacher);
                    await context.SaveChangesAsync();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Views.Teacher.MainView teacherMainView = new(registeredTeacher);
                    Mouse.OverrideCursor = Cursors.Arrow;
                    teacherMainView.Show();

                    Close();
                });
            }

        }

        #region Commands
        private AsyncRelayCommand registerAsyncCommand = null!;
        public AsyncRelayCommand RegisterAsyncCommand
        {
            get => registerAsyncCommand ??= new(async () =>
            {
                if (!RegistrationBackgroundWorker.IsBusy)
                    await RegistrationBackgroundWorker.RunWorkerAsync();
            });
        }

        private RelayCommand switchToAuthenticationNavigationCommand = null!;
        public RelayCommand SwitchToAuthenticationNavigationCommand
        {
            get => switchToAuthenticationNavigationCommand ??= new(() =>
            {
                navigationManager.Navigate(NavigationKeys.Authentication, new NavigationArgs(this));
            });
        }
        #endregion

        #region Validation setup
        private const string UsernameRegexPattern = @"^[a-z0-9]([._-](?![._-])|[a-z0-9]){3,18}[a-z0-9]$";
        private const string PasswordRegexPattern = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,20}$";

        private ValidationState usernameValidationState = ValidationState.Disabled;
        private ValidationState passwordValidationState = ValidationState.Disabled;
        private ValidationState fullNameValidationState = ValidationState.Disabled;

        protected override void SetupValidator()
        {
            ValidationBuilder<RegistrationViewModel> builder = new();
            builder.PropertyCascadeMode = CascadeMode.Stop;

            builder.RuleFor(viewModel => viewModel.Username)
                .NotEmpty()
                .When(viewModel => viewModel.usernameValidationState == ValidationState.Enabled)
                .WithMessage("Поле не может быть пустым");
            builder.RuleFor(viewModel => viewModel.Username)
                .Must(async (username) => !await IsUsernameExistsAsync(username))
                .When(viewModel => viewModel.usernameValidationState == ValidationState.Enabled)
                .WithMessage("Имя пользователя уже занято");
            StringBuilder errorMessageBuilder = new StringBuilder()
                .AppendLine("Имя пользователя должно соответствовать заданным требованиям:")
                .AppendLine("1. Должно состоять из буквенно-цифровых символов (a-z0-9), латинских букв в нижнем регистре")
                .AppendLine("2. Длина от 5 до 20 символов")
                .AppendLine("3. Не может иметь специальных символов, кроме точки (.), подчёркивания (_) и дефиса (-)")
                .AppendLine("4. Точка (.), подчёркивание (_) или дефис (-) не должны появляться подряд (например, user..name)")
                .Append("5. Точка (.), подчёркивание (_) или дефис (-) не должны быть первым или последним символом");
            builder.RuleFor(viewModel => viewModel.Username)
                .Matches(UsernameRegexPattern)
                .When(viewModel => viewModel.usernameValidationState == ValidationState.Enabled)
                .WithMessage(errorMessageBuilder.ToString());

            builder.RuleFor(viewModel => viewModel.Password)
                .NotEmpty()
                .When(viewModel => viewModel.passwordValidationState == ValidationState.Enabled)
                .WithMessage("Поле не может быть пустым");
            errorMessageBuilder = new StringBuilder()
                .AppendLine("Пароль должен соответствовать заданным требованиям:")
                .AppendLine("1. Длина от 8 до 20 символов")
                .AppendLine("2. Должен содержать хотя бы одну цифру [0-9]")
                .AppendLine("3. Должен содержать хотя бы одну латинскую букву нижнего регистра [a-z]")
                .AppendLine("4. Должен содержать хотя бы одну латинскую букву верхнего регистра [A-Z]")
                .Append("5. Должен содержать хотя бы один специальный символ [#?!@$%^&*-]");
            builder.RuleFor(viewModel => viewModel.Password)
                .Matches(PasswordRegexPattern)
                .When(viewModel => viewModel.passwordValidationState == ValidationState.Enabled)
                .WithMessage(errorMessageBuilder.ToString());

            builder.RuleFor(viewModel => viewModel.FullName)
                .NotEmpty()
                .When(viewModel => viewModel.fullNameValidationState == ValidationState.Enabled)
                .WithMessage("ФИО не может быть пустым");
            builder.RuleFor(viewModel => viewModel.FullName)
                .MaxLength(128)
                .When(viewModel => viewModel.fullNameValidationState == ValidationState.Enabled)
                .WithMessage("ФИО не может быть длиннее 128 символов");

            Validator = builder.Build(this);
        }

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

        private async Task<bool> IsFullNameValidAsync()
        {
            fullNameValidationState = ValidationState.Enabled;
            Validator!.Revalidate();
            await Validator.WaitValidatingCompletedAsync();
            fullNameValidationState = ValidationState.Disabled;

            return Validator.IsValid;
        }

        private async Task<bool> IsUsernameExistsAsync(string username)
        {
            await UpdateTeachersFromDatabaseAsync();
            if (teachers.AsParallel().Any(teacher => encoder.Compare(username, teacher.EncryptedName)))
                return true;

            await UpdateStudentsFromDatabaseAsync();
            if (students.AsParallel().Any(student => encoder.Compare(username, student.EncryptedName)))
                return true;

            return false;
        }

        #endregion

    }
}