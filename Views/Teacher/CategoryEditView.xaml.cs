using System.Windows;
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

            viewModel = new CategoryEditViewModel(category);
            viewModel.Closed += (dialogResult) =>
            {
                if (dialogResult is not null)
                    DialogResult = dialogResult;

                Close();
            };

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel.Close();
            };
        }
    }
}