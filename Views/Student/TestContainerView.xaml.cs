using Egor92.MvvmNavigation;
using MvvmBaseViewModels.Helpers;
using MvvmBaseViewModels.Navigation;
using System.Windows;
using TestingSystem.Constants;
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
            Tag = LoadStates.NotLoaded;

            navigationManager = new(FrameContent);

            containerViewModel = new(navigationManager);
            containerViewModel.Closed += (_) => Application.Current?.Dispatcher.Invoke(Close);

            testPassingViewModel = new(navigationManager, test, student);
            testPassingViewModel.Closed += (_) =>
            {
                if (containerViewModel?.IsClosed == false)
                    containerViewModel.Close();
            };
            testPassingViewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            testResultsViewModel = new(navigationManager);
            testResultsViewModel.Closed += (_) =>
            {
                if (containerViewModel?.IsClosed == false)
                    containerViewModel.Close();
            };
            testResultsViewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            testPassingViewModel.InitialLoaderBackgroundWorker.WorkCompleted += () =>
            {
                DataContext = containerViewModel;
                ConfigureNavigation();
                Tag = LoadStates.Loaded;
            };

            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (testPassingViewModel?.IsClosed == false)
                    testPassingViewModel?.Close();

                if (testResultsViewModel?.IsClosed == false)
                    testResultsViewModel?.Close();

                if (containerViewModel?.IsClosed == false)
                    containerViewModel?.Close();
            };
        }

        private void ConfigureNavigation()
        {
            navigationManager.Register<TestPassingView>(NavigationKeys.TestPassing, testPassingViewModel);
            navigationManager.Register<TestResultsView>(NavigationKeys.TestResults, testResultsViewModel);
            navigationManager.Navigate(NavigationKeys.TestPassing, new NavigationArgs(containerViewModel));
        }
    }
}