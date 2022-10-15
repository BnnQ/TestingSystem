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

            viewModel = new TestEditViewModel(test);
            viewModel.Closed += (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };


            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel?.Close();
        }
    }
}