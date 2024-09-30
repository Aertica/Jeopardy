using Jeopardy.Bots;
using Jeopardy.Server.Models;

namespace Jeopardy.Test.Server
{
    [TestFixture]
    public class CategoryTests : TestBase
    {
        [Test]
        [TestCase("\"Hello World!\" - Joseph", true)]
        [TestCase("\"Hello World!\" Joseph", false)]
        [TestCase("\"Hello World! - Joseph", false)]
        [TestCase("Hello World!\" - Joseph", false)]
        [TestCase("Hello World! - Joseph", false)]
        [TestCase("Hello World! Joseph", false)]
        public void TestQuote(string text, bool expected)
        {
            var quote = new Quote(text);
            Assert.That(quote.IsValid, Is.EqualTo(expected), quote.ToString());
        }
    }
}
