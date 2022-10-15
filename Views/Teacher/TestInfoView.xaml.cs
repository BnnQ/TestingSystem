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
            viewModel.Closed += (_) => Close();

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel?.Close();
        }
    }
}