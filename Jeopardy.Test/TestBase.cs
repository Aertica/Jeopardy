using Jeopardy.Bots;
using Jeopardy.Bots.OAuth;
using Newtonsoft.Json;
using NUnit.Framework.Internal;

namespace Jeopardy.Test
{
    [TestFixture]
    public abstract class TestBase
    {
        protected const ulong TEST_GUILD_ID = 974785648298979481; // Bimbo Inc.

        protected static IEnumerable<IBot> Bots { get; set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            Bots = IBot.InitializeBots();
            foreach (var bot in Bots)
                await bot.Ready.Task;
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            
        }
    }
}
