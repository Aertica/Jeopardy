using Jeopardy.Server.Controllers;

namespace Jeopardy.Test
{
    [TestFixture]
    public class ControllerTests
    {
        [Test]
        public void TestGameBoardController()
        {
            var controller = new GameBoardController();
            var result = controller.Create();
            Assert.That(result, Is.Not.Null);
        }
    }
}
