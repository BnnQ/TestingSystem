using MvvmBaseViewModels.Helpers;
using System.Data;
using System.Windows;
using System.Windows.Controls;
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

            viewModel = new TestEditViewModel(test);
            viewModel.Closed += (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };
            viewModel.ErrorMessageOccurred += DefaultMessageHandlers.HandleError;
            viewModel.ErrorMessageOccurred += (_) => Close();
            viewModel.CriticalErrorMessageOccured += DefaultMessageHandlers.HandleCriticalError;
            viewModel.CriticalErrorMessageOccured += (_) => Close();


            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel.Close();
            };
        }

    }
}