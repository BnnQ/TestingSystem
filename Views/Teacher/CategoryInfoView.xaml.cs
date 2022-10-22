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
        private readonly CategoryInfoViewModel viewModel;
        public CategoryInfoView(Category category, Models.Teacher teacher)
        {
            InitializeComponent();

            viewModel = new CategoryInfoViewModel(category, teacher);
            viewModel.Closed += (_) => Close();

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel.Close();
            };
        }
    }
}