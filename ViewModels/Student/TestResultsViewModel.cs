using Egor92.MvvmNavigation.Abstractions;
using MvvmBaseViewModels.Navigation;
using TestingSystem.Models;

namespace TestingSystem.ViewModels.Student
{
    public class TestResultsViewModel : NavigationViewModelBase
    {
        public TestResultsViewModel(INavigationManager navigationManager, TestResults? testResults) : base(navigationManager)
        {

        }

    }
}