using Jeopardy.Server.Models;

namespace Jeopardy.Test.Server
{
    [TestFixture]
    public class ModelTests : TestBase
    {
        //private const int NUM_CATEGORIES = 6;
        //private const int NUM_QUESTIONS_PER_CATEGORY = 5;
        //
        //[Test]
        //public async Task TestCreateGameBoard()
        //{
        //    var gameboard = new GameBoard();
        //    Assert.That(gameboard, Is.Empty);
        //
        //    await gameboard.Reset(Bot, TEST_GUILD_ID, SBot);
        //    Assert.Multiple(() =>
        //    {
        //        Assert.That(gameboard, Is.Not.Null.Or.Empty);
        //        Assert.That(gameboard, Has.Count.EqualTo(NUM_CATEGORIES));
        //        foreach (var category in gameboard)
        //        {
        //            Assert.That(category.Value, Is.Not.Null.Or.Empty);
        //            Assert.That(category.Value.Count(), Is.EqualTo(NUM_QUESTIONS_PER_CATEGORY));
        //            foreach (var question in category.Value)
        //            {
        //                Assert.That(question.Question, Is.Not.Null);
        //                Assert.That(question.Answer, Is.Not.Null);
        //            }
        //        }
        //    });
        //
        //    gameboard = new GameBoard(gameboard.ID);
        //    Assert.Multiple(() =>
        //    {
        //        Assert.That(gameboard, Is.Not.Null.Or.Empty);
        //        Assert.That(gameboard, Has.Count.EqualTo(NUM_CATEGORIES));
        //        foreach (var category in gameboard)
        //        {
        //            Assert.That(category.Value, Is.Not.Null.Or.Empty);
        //            Assert.That(category.Value.Count(), Is.EqualTo(NUM_QUESTIONS_PER_CATEGORY));
        //            foreach (var question in category.Value)
        //            {
        //                Assert.That(question.Question, Is.Not.Null);
        //                Assert.That(question.Answer, Is.Not.Null);
        //            }
        //        }
        //    });
        //}
    }
}