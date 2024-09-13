namespace Jeopardy.Server.Models
{
    public class QuestionCard(IQuestion question, int points)
    {
        public IQuestion Question { get; } = question;

        public int Points { get; } = points;
        
        public State State { get; private set; } = State.Points;

        public void Flip()
        {
            if (State < State.Answer) State++;
        }
    }

    public enum State
    {
        Points,
        Question,
        Answer,
    }
}
