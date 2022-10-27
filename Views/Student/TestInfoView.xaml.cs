using MvvmBaseViewModels.Helpers;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.ViewModels.Student;

namespace TestingSystem.Views.Student
{
    /// <summary>
    /// Interaction logic for TestInfoView.xaml
    /// </summary>
    public partial class TestInfoView : Window
    {
        private TestInfoViewModel viewModel = null!;
        private Test test;
        private Models.Student student;
        public TestInfoView(Test test, Models.Student student)
        {
            InitializeComponent();
            this.test = test;
            this.student = student;
        }

        public void OnViewModelLoaded()
        {
            DataContext = viewModel;
            Tag = ConstantStringKeys.LoadedState;
            viewModel.TestUpdaterFromDatabaseBackgroundWorker.WorkCompleted -= OnViewModelLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Tag = ConstantStringKeys.NotLoadedState;
                viewModel = new TestInfoViewModel(test, student);
                viewModel.Closed += (_) => Application.Current?.Dispatcher.Invoke(Close);
                viewModel.ErrorMessageOccurred += (exception) => DefaultMessageHandlers.HandleError(this, exception);
                viewModel.ErrorMessageOccurred += (_) => Application.Current?.Dispatcher.Invoke(Close);
                viewModel.CriticalErrorMessageOccured += (exception) =>
                    DefaultMessageHandlers.HandleCriticalError(this, exception);

                viewModel.TestUpdaterFromDatabaseBackgroundWorker.WorkCompleted += OnViewModelLoaded;
                Dispatcher.ShutdownStarted += (_, _) =>
                {
                    if (viewModel?.IsClosed == false)
                        viewModel.Close();
                };
            });
        }

    }
}