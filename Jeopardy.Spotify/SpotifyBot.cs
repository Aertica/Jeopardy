using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web;
using Jeopardy.Spotify.OAuth;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Jeopardy.Spotify
{
    public class SpotifyBot
    {
        public SpotifyClient Client { get; set; }
        public TaskCompletionSource<bool> Ready { get; }

        public SpotifyBot()
        {
            //Client = new();
            Ready = new();
        }

        public void StartClient()
        {
            Task.Run(async () =>
            {
                Token? token = Token.Load();
                if (token is null)
                {
                    await WebServer.RequestToken();
                    token = await WebServer.TokenRecieved.Task;
                    token.Save();
                }

                Client = new SpotifyClient(token.AccessToken);
                Ready.SetResult(true);
            });
        }

        private class Token
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
                catch (Exception)
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

        private static class WebServer
        {
            private const string CLIENT_ID = "CLIENT_ID";
            private const string CLIENT_SECRET = "CLIENT_SECRET";
            private const string REDIRECT_URI = "http://localhost:5000/callback";
            private static readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<SpotifyBot>().Build();

            private static EmbedIOAuthServer Server { get; set; }
            public static TaskCompletionSource<Token> TokenRecieved { get; set; }

            public static async Task RequestToken()
            {
                TokenRecieved = new TaskCompletionSource<Token>();
                Server = new EmbedIOAuthServer(new Uri(REDIRECT_URI), 5000);
                await Server.Start();

                Server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

                string clientID = _config[CLIENT_ID]
                    ?? throw new InvalidOperationException($"{CLIENT_ID} was not found in user secrets.");
                var request = new LoginRequest(Server.BaseUri, clientID, LoginRequest.ResponseType.Code)
                {
                    Scope = [Scopes.UserLibraryRead]
                };
                BrowserUtil.Open(request.ToUri());
            }

            private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
            {
                await Server.Stop();

                var config = SpotifyClientConfig.CreateDefault();
                string clientID = _config[CLIENT_ID]
                    ?? throw new InvalidOperationException($"{CLIENT_ID} was not found in user secrets.");
                string clientSecret = _config[CLIENT_SECRET]
                    ?? throw new InvalidOperationException($"{CLIENT_SECRET} was not found in user secrets.");
                var tokenResponse = await new OAuthClient(config).RequestToken(
                    new AuthorizationCodeTokenRequest(clientID, clientSecret, response.Code, new Uri(REDIRECT_URI)));

                TokenRecieved.SetResult(new Token(tokenResponse));
            }
        }
    }
}
