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
        private readonly TestInfoViewModel viewModel;
        public TestInfoView(Test test, Models.Teacher teacher)
        {
            InitializeComponent();

            viewModel = new TestInfoViewModel(test, teacher);
            viewModel.Closed += (_) => Application.Current?.Dispatcher.Invoke(Close);
            viewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel.Close();
            };
        }
    }
}