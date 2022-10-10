using System.Windows;
using TestingSystem.ViewModels.Student;

namespace TestingSystem.Views.Student
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel viewModel;
        public MainView(Models.Student student)
        {
            InitializeComponent();

            viewModel = new MainViewModel(student);
            viewModel.Closed += (_) => Close();

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel?.Dispose();
        }
    }
}