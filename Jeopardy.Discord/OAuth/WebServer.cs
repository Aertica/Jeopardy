using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Jeopardy.Discord.OAuth
{
    public class WebServer
    {
        private const string CLIENT_ID = "CLIENT_ID";
        private const string CLIENT_SECRET = "CLIENT_SECRET";
        private const string USER_ACCESS_TOKEN_ENDPOINT = "https://discord.com/api/v10/oauth2/token";
        private const string REDIRECT_URI = "http://localhost:4000/api/oauth/discord/redirect";
        private readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<WebServer>().Build();

        private HttpClient Client { get; }
        private HttpListener Listener { get; }

        public WebServer()
        {
            Client = new();
            Listener = new();
            Task.Run(Start);
        }

        private async void Start()
        {
            Listener.Prefixes.Add(REDIRECT_URI + "/");
            Listener.Start();
            // wait for a user to authenticate
            while (true)
            {
                var context = await Listener.GetContextAsync();
                await HandleRequest(context.Request, context.Response);
            }
        }

        private async Task HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            var res = await AuthenticateUser(request);

            try
            {
                res.EnsureSuccessStatusCode();

                var accessToken = await res.Content.ReadAsStringAsync();
                var accessTokenInfo = new Token(accessToken);
                await accessTokenInfo.Save();
            }
            catch (Exception) { }
            finally
            {
                response.ContentEncoding ??= Encoding.UTF8;
                var buffer = response.ContentEncoding.GetBytes($"Code {(int)res.StatusCode}: {res.ReasonPhrase}");
                response.ContentLength64 = buffer.Length;
                response.StatusCode = (int)res.StatusCode;
                var stream = response.OutputStream;
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private async Task<HttpResponseMessage> AuthenticateUser(HttpListenerRequest request)
        {
            string clientID = _config[CLIENT_ID]
                ?? throw new InvalidOperationException($"{CLIENT_ID} was not found in user secrets.");
            string clientSecret = _config[CLIENT_SECRET]
                ?? throw new InvalidOperationException($"{CLIENT_SECRET} was not found in user secrets.");
            string code = request.QueryString["code"]
                ?? string.Empty;
            byte[] authBytes = new UTF8Encoding().GetBytes($"{clientID}:{clientSecret}");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", REDIRECT_URI },
                { "scope", "identify connections" },
            });

            return await Client.PostAsync(USER_ACCESS_TOKEN_ENDPOINT, content);
        }
    }
}
