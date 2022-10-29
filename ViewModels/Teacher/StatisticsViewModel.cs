using Egor92.MvvmNavigation.Abstractions;
using MvvmBaseViewModels.Navigation;

namespace TestingSystem.ViewModels.Teacher
{
    public class StatisticsViewModel : NavigationViewModelBase
    {
        public StatisticsViewModel(INavigationManager navigationManager) : base(navigationManager, isRecursiveClosingOfNagivationParentsEnabled: true)
        {

        }
    }
}