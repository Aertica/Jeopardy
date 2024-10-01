using Discord;
using Discord.Rest;
using Jeopardy.Util.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;
using System.Collections.Generic;

namespace Jeopardy.Bots.OAuth
{
    public class Token() : IToken
    {
        private static string Path => $"{Environment.CurrentDirectory}\\AccessTokens.json";

        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        public DateTime? Expiration { get; private set; }

        public Token(string json) : this()
        {
            var token = JObject.Parse(json);
            var accessToken = token.GetValue(nameof(AccessToken))?.ToString()
                ?? throw new JsonReaderException("Access token not found.");
            var refreshToken = token.GetValue(nameof(RefreshToken))?.ToString()
                ?? throw new JsonReaderException("Refresh token not found.");
            var expiresIn = token.GetValue(nameof(Expiration))?.ToString() // Comes in as an int, not a formatted string.
                ?? throw new JsonReaderException("Expiration not found.");

            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Expiration = DateTime.Now.AddSeconds(int.Parse(expiresIn));
        }

        public Token(ulong userID) : this()
        {
            using var reader = new JsonTextReader(File.OpenText(Path));
            var serializer = new JsonSerializer()
            {
                Converters = { new InterfaceConverter<IToken, Token>() },
                Formatting = Formatting.Indented
            };
            var tokens = serializer.Deserialize<Dictionary<ulong, IToken>>(reader)
                ?? throw new JsonReaderException($"Error reading token store at {Path}.");
            var token = tokens[userID];

            AccessToken = token.AccessToken;
            RefreshToken = token.RefreshToken;
            Expiration = token.Expiration;
        }

        public async Task Save()
        {
            using var client = await GetClient();
            var accessTokens = new Dictionary<ulong, IToken>();
            try
            {
                using var reader = new JsonTextReader(File.OpenText(Path));
                var serializer = new JsonSerializer()
                {
                    Converters = { new InterfaceConverter<IToken, Token>() },
                    Formatting = Formatting.Indented
                };
                accessTokens = serializer.Deserialize<Dictionary<ulong, IToken>>(reader)
                    ?? throw new JsonReaderException($"Error reading token store at {Path}.");
            }
            catch (Exception) { }
            finally
            {
                accessTokens.TryAdd(client.CurrentUser.Id, this);
                using var writer = new JsonTextWriter(File.CreateText(Path));
                var serializer = new JsonSerializer()
                {
                    Converters = { new InterfaceConverter<IToken, Token>() },
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(writer, accessTokens);
            }
        }

        public bool IsExpired()
        {
            return Expiration < DateTime.Now;
        }

        public async Task<DiscordRestClient> GetClient()
        {
            var client = new DiscordRestClient();
            await client.LoginAsync(TokenType.Bearer, AccessToken);
            return client;
        }
    }
}
