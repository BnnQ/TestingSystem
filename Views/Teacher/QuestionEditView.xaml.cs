using NeoSmart.AsyncLock;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for QuestionEditView.xaml
    /// </summary>
    public partial class QuestionEditView : Window
    {
        private readonly QuestionEditViewModel viewModel;
        public QuestionEditView(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker, Question question)
        {
            InitializeComponent();

            viewModel = new QuestionEditViewModel(databaseContext, databaseContextLocker, question);
            viewModel.Closed += (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel?.Dispose();
        }
    }
}