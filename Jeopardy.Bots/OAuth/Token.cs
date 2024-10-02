using Discord;
using Discord.Net;
using Discord.Rest;
using Jeopardy.Bots.Exceptions;
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
            var accessToken = token.GetValue("access_token")?.ToString()
                ?? throw new JsonReaderException("Access token not found.");
            var refreshToken = token.GetValue("refresh_token")?.ToString()
                ?? throw new JsonReaderException("Refresh token not found.");
            var expiresIn = token.GetValue("expires_in")?.ToString() // Comes in as an int, not a formatted string.
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
            using var client = await GetClient()
                ?? throw new UnauthorizedException();
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
                accessTokens[client.CurrentUser.Id] = this;
                using var writer = new JsonTextWriter(File.CreateText(Path));
                var serializer = new JsonSerializer()
                { 
                    Converters = { new InterfaceConverter<IToken, Token>() },
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(writer, accessTokens);
            }
        }

        public void Remove()
        {
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
                reader.Close();

                var token = accessTokens.Where(kvp => kvp.Value.AccessToken == AccessToken).Single();

                accessTokens.Remove(token.Key);
                using var writer = new JsonTextWriter(File.CreateText(Path));
                serializer.Serialize(writer, accessTokens);
            }
            catch (Exception) { }
        }

        public bool IsExpired()
        {
            return Expiration < DateTime.Now;
        }

        public async Task<DiscordRestClient?> GetClient()
        {
            try
            {
                var client = new DiscordRestClient();
                await client.LoginAsync(TokenType.Bearer, AccessToken);
                return client;
            }
            catch (HttpException)
            {
                // I'd rather throw an exception here, but they always seem
                // to make the bot freeze rather than return the exception.
                Remove();
                return null;
            }
        }
    }
}
