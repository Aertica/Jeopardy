using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Jeopardy.Spotify.OAuthbhgfe
{
    public class Token
    {
        private static string AccessTokenPath => $"{Environment.CurrentDirectory}\\SpotifyAuth.json";

        public string AccessToken { get; }
        public string RefreshToken { get; }
        public DateTime Expiration { get; }

        public Token()
        {
            using var reader = new JsonTextReader(File.OpenText(AccessTokenPath));
            var token = JToken.ReadFrom(reader)?.ToObject<Dictionary<string, string>>()
                ?? throw new JsonReaderException($"Error reading token store at {AccessTokenPath}.");

            AccessToken = token[nameof(AccessToken)];
            RefreshToken = token[nameof(RefreshToken)];
            Expiration = DateTime.Parse(token[nameof(Expiration)]);
        }

        public Token(AuthorizationCodeTokenResponse response)
        {
            AccessToken = response.AccessToken;
            RefreshToken = response.RefreshToken;
            Expiration = response.CreatedAt.AddSeconds(response.ExpiresIn);
        }

        public void Save()
        {
            using var writer = new JsonTextWriter(File.CreateText(AccessTokenPath));
            var serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };
            serializer.Serialize(writer, this);
        }

        public static Token? Load()
        {
            try
            {
                return new Token();
            }
            catch(Exception)
            {
                return default;
            }
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public bool IsExpired()
        {
            return Expiration < DateTime.Now;
        }
    }
}
