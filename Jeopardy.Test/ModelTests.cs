using Jeopardy.Server.Models;

namespace Jeopardy.Test
{
    public class ModelTests
    {
        [Test]
        public void QuestionCard()
        {
            var card = new QuestionCard(new MockQuestion(), 100);

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
        public void GameBoard()
        {
            var gameboard = new GameBoard();

            Assert.That(gameboard.Game, Is.Not.Null);
        }
    }
}