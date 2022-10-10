using Egor92.MvvmNavigation;
using MvvmBaseViewModels.Navigation;
using NeoSmart.AsyncLock;
using System.Windows;
using TestingSystem.Constants.Authorization;
using TestingSystem.Models.Contexts;
using TestingSystem.ViewModels.Authorization;

namespace TestingSystem.Views.Authorization
{
    /// <summary>
    /// Interaction logic for AuthorizationContainerView.xaml
    /// </summary>
    public partial class AuthorizationContainerView : Window
    {
        private readonly AuthorizationContainerViewModel containerViewModel;
        private readonly TestingSystemAuthorizationContext databaseContext;
        private readonly AsyncLock databaseContextLocker;
        private readonly AuthenticationViewModel authenticationViewModel;
        private readonly RegistrationViewModel registrationViewModel;
        private readonly NavigationManager navigationManager;

        public AuthorizationContainerView()
        {
            InitializeComponent();

            navigationManager = new(FrameContent);
            
            containerViewModel = new(navigationManager);
            containerViewModel.Closed += (_) => Close();

            databaseContext = new TestingSystemAuthorizationContext();
            databaseContextLocker = new AsyncLock();
            authenticationViewModel = new(navigationManager, databaseContext, databaseContextLocker);
            registrationViewModel = new(navigationManager, databaseContext, databaseContextLocker);

            DataContext = containerViewModel;
            ConfigureNavigation();

            Dispatcher.ShutdownStarted += (_, _) =>
            {
                authenticationViewModel?.Close();
                registrationViewModel?.Close();
                databaseContext.Dispose();
            };
        }

        private void ConfigureNavigation()
        {
            navigationManager.Register<AuthenticationView>(NavigationKeys.Authentication, authenticationViewModel);
            navigationManager.Register<RegistrationView>(NavigationKeys.Registration, registrationViewModel);
            navigationManager.Navigate(NavigationKeys.Authentication, new NavigationArgs(containerViewModel));
        }

    }
}