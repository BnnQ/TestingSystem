using MvvmBaseViewModels.Common.Interfaces;
using MvvmBaseViewModels.Navigation;
using TestingSystem.Models;

namespace TestingSystem.Helpers.CustomNavigationArgs
{
    public class TestCompletedNavigationArgs : NavigationArgs
    {
        public TestResult TestResults { get; init; }

        public TestCompletedNavigationArgs(ICloseable? viewModelNavigatedFrom, TestResult testResults) : base(viewModelNavigatedFrom)
        {
            TestResults = testResults;
        }

    }
}