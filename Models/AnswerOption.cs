using CommunityToolkit.Mvvm.ComponentModel;

namespace TestingSystem.Models
{
    public class AnswerOption : ObservableObject
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        private Question question = null!;
        public virtual Question Question
        {
            get => question;
            set
            {
                if (question != value)
                {
                    question = value;
                    OnPropertyChanged(nameof(Question));
                }
            }
        }

        private ushort serialNumberInQuestion;
        public ushort SerialNumberInQuestion
        {
            get => serialNumberInQuestion;
            set
            {
                if (serialNumberInQuestion != value)
                {
                    serialNumberInQuestion = value;
                    OnPropertyChanged(nameof(SerialNumberInQuestion));
                }
            }
        }

        private string content = null!;
        public string Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        private bool isCorrect;
        public bool IsCorrect
        {
            get => isCorrect;
            set
            {
                if (isCorrect != value)
                {
                    isCorrect = value;
                    OnPropertyChanged(nameof(IsCorrect));
                }
            }
        }



        public AnswerOption()
        {
            //empty
        }
        public AnswerOption(Question question, ushort serialNumberInQuestion) : this()
        {
            Question = question;
            SerialNumberInQuestion = serialNumberInQuestion;
            Content = $"{SerialNumberInQuestion} вариант ответа";
        }
        public AnswerOption(Question question, ushort serialNumberInQuestion, string content, bool isCorrect) 
            : this(question, serialNumberInQuestion)
        {
            IsCorrect = isCorrect;
            Content = content;
        }
        

    }
}