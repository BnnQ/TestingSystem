using System.Windows;
using TestingSystem.Models;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for AnswerOptionEditView.xaml
    /// </summary>
    public partial class AnswerOptionEditView : Window
    {
        private readonly AnswerOptionEditViewModel viewModel;
        public AnswerOptionEditView(AnswerOption answerOption)
        {
            InitializeComponent();

            viewModel = new AnswerOptionEditViewModel(answerOption);
            viewModel.Closed += (dialogResult) =>
            {
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