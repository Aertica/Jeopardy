using Discord;
using Discord.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jeopardy.Discord.OAuth
{
    public class Token
    {
        private static string AccessTokenPath => $"{Environment.CurrentDirectory}\\AccessTokens.json";

        public string AccessToken { get; }
        public string RefreshToken { get; }
        public DateTime Expiration { get; }

        public Token(string json)
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

        public Token(ulong userID)
        {
            using var reader = new JsonTextReader(File.OpenText(AccessTokenPath));
            var tokens = JToken.ReadFrom(reader)?.ToObject<Dictionary<ulong, Dictionary<string, string>>>()
                ?? throw new JsonReaderException($"Error reading token store at {AccessTokenPath}.");
            var token = tokens[userID];

            AccessToken = token[nameof(AccessToken)];
            RefreshToken = token[nameof(RefreshToken)];
            Expiration = DateTime.Parse(token[nameof(Expiration)]);
        }

        public async Task Save()
        {
            using var client = await GetRestClient();
            var accessTokens = new Dictionary<ulong, Dictionary<string, string>>();

            try
            {
                using var reader = new JsonTextReader(File.OpenText(AccessTokenPath));
                accessTokens = JToken.ReadFrom(reader)?.ToObject<Dictionary<ulong, Dictionary<string, string>>>()
                    ?? throw new JsonReaderException($"Error reading token store at {AccessTokenPath}.");
            }
            catch (Exception) { }
            finally
            {
                accessTokens.TryAdd(client.CurrentUser.Id, new Dictionary<string, string>()
                {
                    { nameof(AccessToken), AccessToken },
                    { nameof(RefreshToken), RefreshToken },
                    { nameof(Expiration), Expiration.ToString() }
                });

                using var writer = new JsonTextWriter(File.CreateText(AccessTokenPath));
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, accessTokens);
            }
        }

        public bool IsExpired()
        {
            return Expiration < DateTime.Now;
        }

        public async Task<DiscordRestClient> GetRestClient()
        {
            var client = new DiscordRestClient();
            await client.LoginAsync(TokenType.Bearer, AccessToken);
            return client;
        }
    }
}
