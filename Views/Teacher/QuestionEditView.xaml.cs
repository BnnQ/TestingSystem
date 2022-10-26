using MvvmBaseViewModels.Helpers;
using System.Windows;
using TestingSystem.Models;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for QuestionEditView.xaml
    /// </summary>
    public partial class QuestionEditView : Window
    {
        private readonly QuestionEditViewModel viewModel;
        public QuestionEditView(Question question)
        {
            InitializeComponent();

            viewModel = new QuestionEditViewModel(question);
            viewModel.Closed += (dialogResult) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (dialogResult is not null)
                        DialogResult = dialogResult;

                    Close();
                });
            };

            DataContext = viewModel;
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel?.Close();
            };
        }
    }
}