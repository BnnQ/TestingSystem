using MvvmBaseViewModels.Helpers;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for TestInfoView.xaml
    /// </summary>
    public partial class TestInfoView : Window
    {
        private TestInfoViewModel viewModel = null!;
        private Test test;
        private Models.Teacher teacher;
        public TestInfoView(Test test, Models.Teacher teacher)
        {
            InitializeComponent();
            this.test = test;
            this.teacher = teacher;
        }

        private void OnViewModelLoaded()
        {
            DataContext = viewModel;
            Tag = ConstantStringKeys.LoadedState;
            viewModel.TestUpdaterFromDatabaseBackgroundWorker.WorkCompleted -= OnViewModelLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Tag = ConstantStringKeys.NotLoadedState;

                viewModel = new TestInfoViewModel(test, teacher);
                viewModel.Closed += (dialogResult) =>
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        if (dialogResult is not null)
                            DialogResult = dialogResult;

                        Close();
                    });
                };
                viewModel.ErrorMessageOccurred += DefaultMessageHandlers.HandleError;
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