using MvvmBaseViewModels.Helpers;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for CategoryInfoView.xaml
    /// </summary>
    public partial class CategoryInfoView : Window
    {
        private CategoryInfoViewModel viewModel = null!;
        private Category category;
        private Models.Teacher teacher;
        public CategoryInfoView(Category category, Models.Teacher teacher)
        {
            InitializeComponent();
            this.category = category;
            this.teacher = teacher;
        }

        public void OnViewModelLoaded()
        {
            DataContext = viewModel;
            Tag = ConstantStringKeys.LoadedState;
            viewModel.CategoryUpdaterFromDatabaseBackgroundWorker.WorkCompleted -= OnViewModelLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Tag = ConstantStringKeys.NotLoadedState;
                viewModel = new CategoryInfoViewModel(category, teacher);
                viewModel.Closed += (_) => Application.Current?.Dispatcher.Invoke(Close);
                viewModel.CriticalErrorMessageOccured += (exception) =>
                    DefaultMessageHandlers.HandleCriticalError(this, exception);

                viewModel.CategoryUpdaterFromDatabaseBackgroundWorker.WorkCompleted += OnViewModelLoaded;
                Dispatcher.ShutdownStarted += (_, _) =>
                {
                    if (viewModel?.IsClosed == false)
                        viewModel.Close();
                };
            });
        }
    }
}