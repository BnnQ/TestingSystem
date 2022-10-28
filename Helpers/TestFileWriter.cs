using System.IO;
using System.Text;
using System.Threading.Tasks;
using TestingSystem.Models;

namespace TestingSystem.Helpers
{
    public static class TestFileWriter
    {
        public static async Task WriteToTextFileAsync(Test test, string path, bool includeCorrectAnswers = false)
        {
            string testString = null!;
            await Task.Run(() =>
            {
                StringBuilder testStringBuilder = new StringBuilder()
                                                  .AppendLine(test.Name)
                                                  .AppendLine();

                foreach (Question question in test.Questions)
                {
                    testStringBuilder.AppendLine($"{question.SerialNumberInTest}) {question.Content}");
                    foreach (AnswerOption answerOption in question.AnswerOptions)
                        testStringBuilder.AppendLine($"{(includeCorrectAnswers && answerOption.IsCorrect ? '✓' : ' ')}{answerOption.SerialNumberInQuestion}. {answerOption.Content}");

                    testStringBuilder
                        .AppendLine()
                        .AppendLine();
                }

                testString = testStringBuilder.ToString();
            });

            await File.WriteAllTextAsync(path, testString);
        }
    }
}