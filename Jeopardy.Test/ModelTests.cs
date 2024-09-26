using Jeopardy.Server.Models;

namespace Jeopardy.Test
{
    [TestFixture]
    public class ModelTests
    {
        private const int NUM_CATEGORIES = 6;
        private const int NUM_QUESTIONS_PER_CATEGORY = 5;

        [Test]
        public void TestQuestionCard()
        {
            var question = new MockQuestion();
            var card = new QuestionCard(question, 100);

            Assert.Multiple(() =>
            {
                Assert.That(card.Question.Question, Is.Not.Empty);
                Assert.That(card.Question.Answer, Is.Not.Empty);
                Assert.That(card.Points, Is.Not.Zero);
                Assert.That(card.State, Is.EqualTo(State.Points));
            });
            card.Flip();
            Assert.That(card.State, Is.EqualTo(State.Question));
            card.Flip();
            Assert.That(card.State, Is.EqualTo(State.Answer));
            card.Flip();
            Assert.That(card.State, Is.EqualTo(State.Answer));
        }

        [Test]
        public void TestGameBoard()
        {
            var gameboard = new GameBoard();

            Assert.Multiple(() =>
            {
                Assert.That(gameboard.Game, Is.Not.Null);
                Assert.That(gameboard.Game, Has.Count.EqualTo(NUM_CATEGORIES));
                foreach (var category in gameboard.Game)
                {
                    Assert.That(category.Value.Count(), Is.EqualTo(NUM_QUESTIONS_PER_CATEGORY));
                }
            });
        }
    }
}