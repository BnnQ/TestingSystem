using MvvmBaseViewModels.Helpers;
using System.Windows;
using System.Windows.Input;
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
            viewModel.CriticalErrorMessageOccured += DefaultMessageHandlers.HandleCriticalError;
            viewModel.CriticalErrorMessageOccured += (_) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Close();
                });
            };


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