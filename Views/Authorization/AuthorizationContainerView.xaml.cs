using Egor92.MvvmNavigation;
using MvvmBaseViewModels.Navigation;
using System.Windows;
using TestingSystem.Constants.Authorization;
using TestingSystem.ViewModels.Authorization;

namespace TestingSystem.Views.Authorization
{
    /// <summary>
    /// Interaction logic for AuthorizationContainerView.xaml
    /// </summary>
    public partial class AuthorizationContainerView : Window
    {
        private readonly AuthorizationContainerViewModel containerViewModel;
        private readonly AuthenticationViewModel authenticationViewModel;
        private readonly RegistrationViewModel registrationViewModel;
        private readonly NavigationManager navigationManager;

        public AuthorizationContainerView()
        {
            InitializeComponent();

            navigationManager = new(FrameContent);
            
            containerViewModel = new(navigationManager);
            containerViewModel.Closed += (_) => Close();

            authenticationViewModel = new(navigationManager);
            authenticationViewModel.Closed += (_) =>
            {
                if (containerViewModel is not null && !containerViewModel.IsClosed)
                    containerViewModel.Close();
            };

            registrationViewModel = new(navigationManager);
            registrationViewModel.Closed += (_) =>
            {
                if (containerViewModel is not null && !containerViewModel.IsClosed)
                    containerViewModel.Close();
            };

            DataContext = containerViewModel;
            ConfigureNavigation();

            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (authenticationViewModel is not null && !authenticationViewModel.IsClosed)
                    authenticationViewModel?.Close();
                
                if (registrationViewModel is not null && !registrationViewModel.IsClosed)
                    registrationViewModel?.Close();
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