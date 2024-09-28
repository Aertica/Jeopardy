namespace Jeopardy.Server.Models
{
    public class BaseQuestion() : IQuestion
    {
        public virtual string Question { get; private set; } = "Question";

        public virtual string Answer { get; private set; } = "Answer";

        public static IEnumerable<IQuestion> FetchQuestions(int count = 5)
        {
            return Enumerable.Repeat(new BaseQuestion(), count);
        }
    }
}
