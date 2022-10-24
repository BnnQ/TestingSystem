using Egor92.MvvmNavigation;
using MvvmBaseViewModels.Helpers;
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
            containerViewModel.Closed += (_) => Application.Current?.Dispatcher.Invoke(Close);

            authenticationViewModel = new(navigationManager);
            authenticationViewModel.Closed += (_) =>
            {
                if (containerViewModel?.IsClosed == false)
                    containerViewModel.Close();
            };
            authenticationViewModel.CriticalErrorMessageOccured += (exception) => 
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            registrationViewModel = new(navigationManager);
            registrationViewModel.Closed += (_) =>
            {
                if (containerViewModel?.IsClosed == false)
                    containerViewModel.Close();
            };
            registrationViewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            DataContext = containerViewModel;
            ConfigureNavigation();

            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (authenticationViewModel?.IsClosed == false)
                    authenticationViewModel?.Close();
                
                if (registrationViewModel?.IsClosed == false)
                    registrationViewModel?.Close();

                if (containerViewModel?.IsClosed == false)
                    containerViewModel?.Close();
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