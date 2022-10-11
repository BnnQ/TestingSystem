using NeoSmart.AsyncLock;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.Models.Contexts;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for AnswerOptionEditView.xaml
    /// </summary>
    public partial class AnswerOptionEditView : Window
    {
        private readonly AnswerOptionEditViewModel viewModel;
        public AnswerOptionEditView(TestingSystemTeacherContext databaseContext, AsyncLock databaseContextLocker, AnswerOption answerOption)
        {
            InitializeComponent();

            viewModel = new AnswerOptionEditViewModel(databaseContext, databaseContextLocker, answerOption);
            viewModel.Closed += (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };
              

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) => viewModel.Dispose();
        }
    }
}