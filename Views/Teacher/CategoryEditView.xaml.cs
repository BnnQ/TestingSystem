using MvvmBaseViewModels.Helpers;
using System.Windows;
using System.Windows.Input;
using TestingSystem.Models;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for CategoryEditView.xaml
    /// </summary>
    public partial class CategoryEditView : Window
    {
        private readonly CategoryEditViewModel viewModel;
        public CategoryEditView(Category category)
        {
            InitializeComponent();
            Application.Current?.Dispatcher.Invoke(() =>
            {
                IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
            });

            viewModel = new CategoryEditViewModel(category);
            viewModel.Closed += (dialogResult) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (dialogResult is not null)
                        DialogResult = dialogResult;

                    Close();
                });
            };
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
    }
}