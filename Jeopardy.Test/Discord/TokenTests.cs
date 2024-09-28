using Jeopardy.Discord.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Test.Discord
{
    [TestFixture]
    public class TokenTests : TestBase
    {
        [Test]
        [TestCase(468965253993201696u)]
        public async Task TestTokenFromID(ulong id)
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
        public void TestTokenFromValidJSON(string json)
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
        public void TestTokenFromInvalidJSON(string json)
        {
            Assert.That(() => new Token(json), Throws.TypeOf<JsonReaderException>());
        }
    }
}
