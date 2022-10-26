using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Navigation;
using TestingSystem.Helpers.CustomNavigationArgs;
using TestingSystem.Models;

namespace TestingSystem.ViewModels.Student
{
    public class TestResultsViewModel : NavigationViewModelBase
    {
        private TestResults? testResults;
        public TestResults? TestResults
        {
            get => testResults;
            set
            {
                if (testResults != value)
                {
                    testResults = value;
                    OnPropertyChanged(nameof(TestResults));
                }
            }
        }


        public TestResultsViewModel(INavigationManager navigationManager) : base(navigationManager)
        {
            //empty
        }


        public override void OnNavigatedTo(object arg)
        {
            base.OnNavigatedTo(arg);
            if (arg is TestCompletedNavigationArgs testCompletedArgs)
                TestResults = testCompletedArgs.TestResults;
        }

        #region Commands
        private RelayCommand okCommand = null!;
        public RelayCommand OkCommand
        {
            get => okCommand ??= new(() => Close());
        }
        #endregion

    }
}