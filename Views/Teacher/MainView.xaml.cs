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
            Application.Current?.Dispatcher.Invoke(() =>
            {
                IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
            });

            viewModel = new MainViewModel(teacher);
            viewModel.Closed += (_) => Close();
            viewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            viewModel.InitialLoaderBackgroundWorker.WorkCompleted += () =>
            {
                DataContext = viewModel;
                IsEnabled = true;
                Mouse.OverrideCursor = Cursors.Arrow;
            };
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