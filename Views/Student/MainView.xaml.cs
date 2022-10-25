using MvvmBaseViewModels.Helpers;
using System.Windows;
using System.Windows.Input;
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
            viewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel.Close();
            };
        }

        private void OnReferenceElementMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void OnReferenceElementMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}