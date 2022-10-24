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
                if (dialogResult is not null)
                    DialogResult = dialogResult;

                Application.Current?.Dispatcher.Invoke(Close);
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