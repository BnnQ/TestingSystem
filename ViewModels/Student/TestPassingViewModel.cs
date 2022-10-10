using Egor92.MvvmNavigation.Abstractions;
using MvvmBaseViewModels.Navigation;
using TestingSystem.Models;

namespace TestingSystem.ViewModels.Student
{
    public class TestPassingViewModel : NavigationViewModelBase
    {
        public TestPassingViewModel(INavigationManager navigationManager, Test test, Models.Student student) : base(navigationManager)
        {

        }

    }
}