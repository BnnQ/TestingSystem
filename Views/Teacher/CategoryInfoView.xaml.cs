using NeoSmart.AsyncLock;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for CategoryInfoView.xaml
    /// </summary>
    public partial class CategoryInfoView : Window
    {
        private readonly CategoryInfoViewModel viewModel;
        public CategoryInfoView(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker, 
            Category category, Models.Teacher teacher)
        {
            InitializeComponent();

            viewModel = new CategoryInfoViewModel(databaseContext, databaseContextLocker, category, teacher);
            viewModel.Closed += (_) => Close();

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel?.Dispose();
        }
    }
}