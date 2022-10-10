using Egor92.MvvmNavigation;
using MvvmBaseViewModels.Navigation;
using System.Windows;
using TestingSystem.Constants.Student;
using TestingSystem.Models;
using TestingSystem.ViewModels.Student;

namespace TestingSystem.Views.Student
{
    /// <summary>
    /// Interaction logic for TestContainerView.xaml
    /// </summary>
    public partial class TestContainerView : Window
    {
        private readonly TestContainerViewModel containerViewModel;
        private readonly TestPassingViewModel testPassingViewModel;
        private readonly TestResultsViewModel testResultsViewModel;
        private readonly NavigationManager navigationManager;

        public TestContainerView(Test test, Models.Student student)
        {
            InitializeComponent();

            navigationManager = new(FrameContent);

            containerViewModel = new(navigationManager, test);
            containerViewModel.Closed += (_) => Close();

            testPassingViewModel = new(navigationManager, test, student);
            testResultsViewModel = new(navigationManager, null);

            DataContext = containerViewModel;
            ConfigureNavigation();
        }

        private void ConfigureNavigation()
        {
            navigationManager.Register<TestPassingView>(NavigationKeys.TestPassing, testPassingViewModel);
            navigationManager.Register<TestResultsView>(NavigationKeys.TestResults, testResultsViewModel);
            navigationManager.Navigate(NavigationKeys.TestPassing, new NavigationArgs(containerViewModel));
        }
    }
}