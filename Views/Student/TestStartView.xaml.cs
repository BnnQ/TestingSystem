using System.Windows;
using TestingSystem.Models;
using TestingSystem.ViewModels.Student;

namespace TestingSystem.Views.Student
{
    /// <summary>
    /// Interaction logic for TestStartView.xaml
    /// </summary>
    public partial class TestStartView : Window
    {
        private readonly TestStartViewModel viewModel;
        public TestStartView(Test test, Models.Student student)
        {
            InitializeComponent();

            viewModel = new TestStartViewModel(test, student);
            viewModel.Closed += (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel?.Dispose();
        }
    }
}