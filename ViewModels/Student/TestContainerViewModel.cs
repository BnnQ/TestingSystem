using Egor92.MvvmNavigation.Abstractions;
using MvvmBaseViewModels.Common;

namespace TestingSystem.ViewModels.Student
{
    public class TestContainerViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        public TestContainerViewModel(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

    }
}