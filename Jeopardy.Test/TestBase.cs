using Jeopardy.Discord;
using Jeopardy.Discord.OAuth;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jeopardy.Test
{
    [TestFixture]
    public abstract class TestBase
    {
        protected const ulong TEST_GUILD_ID = 974785648298979481; // Bimbo Inc.

        protected static DiscordBot Bot { get; } = new();
        protected static WebServer Server { get; } = new();

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            using var writer = new JsonTextWriter(File.CreateText($"{Environment.CurrentDirectory}\\ActiveGames.json"));

            Bot.StartClient();
            Server.Start();
            await Bot.Ready.Task;
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Bot.Client.LogoutAsync();
            Server.Dispose();
        }
    }
}
