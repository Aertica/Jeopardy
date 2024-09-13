namespace Jeopardy.Server.Models
{
    public class GameBoard
    {
        public Dictionary<string, IEnumerable<QuestionCard>> Game { get; set; } = new()
        {
            { "Category 1", GetMockCategory() },
            { "Category 2", GetMockCategory() },
            { "Category 3", GetMockCategory() },
            { "Category 4", GetMockCategory() },
            { "Category 5", GetMockCategory() },
            { "Category 6", GetMockCategory() },
        };

        public static IEnumerable<QuestionCard> GetMockCategory()
        {
            return from n in Enumerable.Range(1, 5) select new QuestionCard(new MockQuestion(), n * 100);
        }
    }
}
