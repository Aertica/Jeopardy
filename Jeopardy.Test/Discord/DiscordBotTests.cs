using Jeopardy.Discord;
using Jeopardy.Discord.OAuth;
using Jeopardy.Server.Models;
using Newtonsoft.Json;
using System.ComponentModel.Design;

namespace Jeopardy.Test.Discord
{
    [TestFixture]
    public class DiscordBotTests : TestBase
    {
        [Test]
        public async Task TestFetchQuotes()
        {
            var quotes = await Bot.FetchQuotes(TEST_GUILD_ID);

            Assert.That(quotes, Is.Not.Empty);
            foreach (var quote in quotes)
            {
                Assert.That(quote.IsValid, quote.ToString());
            }
        }
    }
}
