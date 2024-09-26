using Jeopardy.Discord;
using Jeopardy.Discord.OAuth;
using Newtonsoft.Json;

namespace Jeopardy.Test
{
    [TestFixture]
    public class DiscordTests
    {
        private const ulong AUTHORIZE_COMMAND_ID = 1166549041816010792;
        private const ulong PLAY_COMMAND_ID = 1115399744597008518;

        private static DiscordBot Bot { get; set; } = new();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Bot.StartClient();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await Bot.Client.LogoutAsync();
        }

        [Test]
        public async Task TestAuthorizeCommand()
        {
            await Bot.Ready.Task;
            var command = await Bot.Client.GetGlobalApplicationCommandAsync(AUTHORIZE_COMMAND_ID)
                ?? throw new ArgumentException("Command id is invalid.", nameof(AUTHORIZE_COMMAND_ID));

            // TODO: I have not figured out if there is a way to test commands using a SocketSlashCommand object
            Assert.Ignore();
        }

        [Test]
        public async Task TestPlayCommand()
        {
            await Bot.Ready.Task;
            var command = await Bot.Client.GetGlobalApplicationCommandAsync(PLAY_COMMAND_ID)
                ?? throw new ArgumentException("Command id is invalid.", nameof(PLAY_COMMAND_ID));

            // TODO: I have not figured out if there is a way to test commands using a SocketSlashCommand object
            Assert.Ignore();
        }

        [Test]
        [TestCase(468965253993201696u)]
        public async Task TestOAuthTokenFromValidID(ulong id)
        {
            var token = new Token(id);
            Assert.Multiple(() =>
            {
                Assert.That(token, Is.Not.Null);
                Assert.That(token.AccessToken, Is.Not.Null.Or.Empty);
                Assert.That(token.RefreshToken, Is.Not.Null.Or.Empty);
            });

            if (!token.IsExpired()) // TODO: Figure out how to ensure test users' tokens are always valid
            {
                await token.Save();
            }
            else
            {
                Assert.Ignore("Stored token is expired.");
            }
        }

        [Test]
        [TestCase(@"{""AccessToken"":""ABCDEF"",""RefreshToken"":""abcdef"",""Expiration"":""0""}")]
        public void TestOAuthTokenFromValidJSONSchema(string json)
        {
            var token = new Token(json);
            Assert.Multiple(() =>
            {
                Assert.That(token, Is.Not.Null);
                Assert.That(token.AccessToken, Is.Not.Null.Or.Empty);
                Assert.That(token.RefreshToken, Is.Not.Null.Or.Empty);
            });
        }

        [Test]
        [TestCase(@"{""RefreshToken"":""abcdef"",""Expiration"":""0""}")]
        [TestCase(@"{""AccessToken"":""ABCDEF"",""Expiration"":""0""}")]
        [TestCase(@"{""AccessToken"":""ABCDEF"",""RefreshToken"":""abcdef"",}")]
        public void TestOAuthTokenFromInvalidJSONSchema(string json)
        {
            Assert.That(() => { _ = new Token(json); }, Throws.TypeOf<JsonReaderException>());
        }
    }
}
