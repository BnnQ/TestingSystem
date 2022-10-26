using MvvmBaseViewModels.Helpers;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for TestEditView.xaml
    /// </summary>
    public partial class TestEditView : Window
    {
        private readonly TestEditViewModel viewModel;
        public TestEditView(Test test)
        {
            InitializeComponent();
            Tag = ConstantStringKeys.NotLoadedState;

            viewModel = new TestEditViewModel(test);
            viewModel.Closed += (dialogResult) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (dialogResult is not null)
                        DialogResult = dialogResult;

                    Close();
                });
            };
            viewModel.ErrorMessageOccurred += (exception) => DefaultMessageHandlers.HandleError(this, exception);
            viewModel.ErrorMessageOccurred += (_) => Application.Current?.Dispatcher.Invoke(Close);
            viewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            viewModel.InitialLoaderBackgroundWorker.WorkCompleted += () =>
            {
                DataContext = viewModel;
                Tag = ConstantStringKeys.LoadedState;
            };
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel.Close();
            };
        }

    }
}