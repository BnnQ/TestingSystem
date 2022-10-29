using Egor92.MvvmNavigation;
using MvvmBaseViewModels.Helpers;
using MvvmBaseViewModels.Navigation;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Constants.Teacher;
using TestingSystem.ViewModels;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for MainContainerView.xaml
    /// </summary>
    public partial class MainContainerView : Window
    {
        private readonly MainContainerViewModel containerViewModel;
        private readonly MainViewModel mainViewModel;
        private readonly StatisticsViewModel statisticsViewModel;
        private readonly AboutViewModel aboutViewModel;
        private readonly NavigationManager navigationManager;

        private bool statisticsLoaded = false;
        private bool mainLoaded = false;

        public MainContainerView(Models.Teacher teacher)
        {
            InitializeComponent();
            Application.Current?.Dispatcher.Invoke(() =>
            {
                IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
            });

            navigationManager = new(FrameContent);

            containerViewModel = new(navigationManager, teacher);
            containerViewModel.Closed += (_) => Application.Current?.Dispatcher.Invoke(Close);

            mainViewModel = new(navigationManager, teacher);
            mainViewModel.Closed += (_) =>
            {
                if (containerViewModel?.IsClosed == false)
                    containerViewModel.Close();
            };
            mainViewModel.ErrorMessageOccurred += (exception) => DefaultMessageHandlers.HandleError(this, exception);
            mainViewModel.CriticalErrorMessageOccured += (exception) => 
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            statisticsViewModel = new(navigationManager, teacher);
            statisticsViewModel.Closed += (_) =>
            {
                if (containerViewModel?.IsClosed == false)
                    containerViewModel.Close();
            };
            statisticsViewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            aboutViewModel = new();
            aboutViewModel.Closed += (_) =>
            {
                if (containerViewModel?.IsClosed == false)
                    containerViewModel.Close();
            };

            mainViewModel.InitialLoaderBackgroundWorker.WorkCompleted += OnMainLoaded;
            statisticsViewModel.DataUpdaterFromDatabaseBackgroundWorker.WorkCompleted += OnStatisticsLoaded;
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (mainViewModel?.IsClosed == false)
                    mainViewModel?.Close();

                if (statisticsViewModel?.IsClosed == false)
                    statisticsViewModel?.Close();

                if (aboutViewModel?.IsClosed == false)
                    aboutViewModel?.Close();

                if (containerViewModel?.IsClosed == false)
                    containerViewModel?.Close();
            };
        }

        private void CompleteInitialization()
        {
            ConfigureNavigation();
            Application.Current?.Dispatcher.Invoke(() =>
            {
                DataContext = containerViewModel;
                IsEnabled = true;
                Mouse.OverrideCursor = Cursors.Arrow;
            });
        }

        private void OnMainLoaded()
        {
            mainLoaded = true;
            if (!statisticsLoaded)
                return;
            else
                CompleteInitialization();
        }

        private void OnStatisticsLoaded()
        {
            statisticsLoaded = true;
            statisticsViewModel.DataUpdaterFromDatabaseBackgroundWorker.WorkCompleted -= OnStatisticsLoaded;
            if (!mainLoaded)
                return;
            else
                CompleteInitialization();
        }

        private void ConfigureNavigation()
        {
            navigationManager.Register<MainView>(NavigationKeys.Main, mainViewModel);
            navigationManager.Register<StatisticsView>(NavigationKeys.Statistics, statisticsViewModel);
            navigationManager.Register<AboutView>(NavigationKeys.About, aboutViewModel);
            
            navigationManager.Navigate(NavigationKeys.Main, new NavigationArgs(containerViewModel));
        }

    }
}