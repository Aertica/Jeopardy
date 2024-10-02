using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Bots.OAuth
{
    public class AuthServer
    {
        private const string DISCORD_CLIENT_ID = "DISCORD_CLIENT_ID";
        private const string DISCORD_CLIENT_SECRET = "DISCORD_CLIENT_SECRET";
        private const string USER_ACCESS_TOKEN_ENDPOINT = "https://discord.com/api/oauth2/token";
        private readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<AuthServer>().Build();

        private HttpClient Client { get; }
        private HttpListener Listener { get; }
        private IEnumerable<string> RedirectURIs { get; }

        public AuthServer(IEnumerable<string> uris)
        {
            Client = new();
            Listener = new();
            RedirectURIs = uris;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                foreach (var uri in RedirectURIs)
                    Listener.Prefixes.Add(uri + "/");
                Listener.Start();
                // wait for a user to authenticate
                while (true)
                {
                    var context = await Listener.GetContextAsync();
                    await HandleRequest(context.Request, context.Response);
                }
            });
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
            string clientID = _config[DISCORD_CLIENT_ID]
                ?? throw new InvalidOperationException($"{DISCORD_CLIENT_ID} was not found in user secrets.");
            string clientSecret = _config[DISCORD_CLIENT_SECRET]
                ?? throw new InvalidOperationException($"{DISCORD_CLIENT_SECRET} was not found in user secrets.");
            string code = request.QueryString["code"] ?? string.Empty;
            byte[] authBytes = new UTF8Encoding().GetBytes($"{clientID}:{clientSecret}");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", RedirectURIs.First() },
                { "scope", "identify connections" },
            });

            return await Client.PostAsync(USER_ACCESS_TOKEN_ENDPOINT, content);
        }
    }
}
