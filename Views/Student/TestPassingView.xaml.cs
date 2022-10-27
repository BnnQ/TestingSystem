using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TestingSystem.Models;
using TestingSystem.ViewModels.Student;

namespace TestingSystem.Views.Student
{
    /// <summary>
    /// Interaction logic for TestPassingView.xaml
    /// </summary>
    public partial class TestPassingView : UserControl
    {
        private Brush primaryColor;
        private Brush secondaryColor;
        public TestPassingView()
        {
            InitializeComponent();
            
            primaryColor = (Application.Current?.FindResource("PrimaryHueMidBrush") as Brush)!;
            secondaryColor = (Application.Current?.FindResource("SecondaryHueMidBrush") as Brush)!;
        }


        private void OnAnswerOptionButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button senderButton)
            {
                if (senderButton.Tag is ushort serialNumberInQuestion)
                {
                    if (DataContext is TestPassingViewModel viewModel)
                    {
                        AnswerOption answerOption = viewModel.CurrentQuestionAnswerOptions.First(answerOption => answerOption.SerialNumberInQuestion == serialNumberInQuestion);
                        
                        if (viewModel.ChooseAnswerOptionCommand.CanExecute(answerOption))
                        {
                            viewModel.ChooseAnswerOptionCommand.Execute(answerOption);

                            if (viewModel.SelectedAnswerOptions.Contains(answerOption))
                                Application.Current?.Dispatcher.Invoke(() => senderButton.Background = secondaryColor);
                            else
                                Application.Current?.Dispatcher.Invoke(() => senderButton.Background = primaryColor);
                        }
                    }
                }
            }
        }
    }
}