using MvvmBaseViewModels.Common.Interfaces;
using MvvmBaseViewModels.Navigation;
using TestingSystem.Models;

namespace TestingSystem.Helpers.CustomNavigationArgs
{
    public class TestCompletedNavigationArgs : NavigationArgs
    {
        public TestResults TestResults { get; init; }

        public TestCompletedNavigationArgs(ICloseable? viewModelNavigatedFrom, TestResults testResults) : base(viewModelNavigatedFrom)
        {
            TestResults = testResults;
        }

    }
}