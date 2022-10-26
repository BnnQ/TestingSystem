using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using TestingSystem.ViewModels.Student;

namespace TestingSystem.Views.Student
{
    /// <summary>
    /// Interaction logic for TestPassingView.xaml
    /// </summary>
    public partial class TestPassingView : UserControl
    {
        public TestPassingView()
        {
            InitializeComponent();
        }

        private Brush initialButtonBackground = Brushes.RoyalBlue;
        private void OnAnswerOptionClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button buttonSender)
            {
                TestPassingViewModel dataContext = (DataContext as TestPassingViewModel)!;

                if (buttonSender.Background != Brushes.Orange)
                {
                    if (dataContext.DoesCurrentQuestionOnlyHaveOneCorrectAnswer && dataContext.SelectedAnswerOptions.Any())
                        return;

                    initialButtonBackground = buttonSender.Background;
                    buttonSender.Background = Brushes.Orange;
                }
                else
                {
                    buttonSender.Background = initialButtonBackground;
                }
            }
        }

    }
}