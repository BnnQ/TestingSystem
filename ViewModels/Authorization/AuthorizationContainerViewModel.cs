using Egor92.MvvmNavigation.Abstractions;
using MvvmBaseViewModels.Common;

namespace TestingSystem.ViewModels.Authorization
{
    public class AuthorizationContainerViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        public AuthorizationContainerViewModel(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }
    }
}