using System.Windows;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel viewModel;
        public MainView(Models.Teacher teacher)
        {
            InitializeComponent();

            viewModel = new MainViewModel(teacher);
            viewModel.Closed += (_) => Close();

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel?.Dispose();
        }
    }
}