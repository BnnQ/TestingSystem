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
                    viewModel.Close();
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (Width > SystemParameters.PrimaryScreenWidth)
                    Width = SystemParameters.PrimaryScreenWidth;
                if (Height > SystemParameters.PrimaryScreenHeight)
                    Height = SystemParameters.PrimaryScreenHeight;

                firstTextBox.Focus();
            });
        }

    }
}