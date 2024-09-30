using Jeopardy.Bots.OAuth;
using Jeopardy.Util;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Http;
using Swan;
using Swan.Parsers;

namespace Jeopardy.Bots
{
    public class SpotifyBot : IBot
    {
        private const string SPOTIFY_CLIENT_ID = "SPOTIFY_CLIENT_ID";
        private const string SPOTIFY_CLIENT_SECRET = "SPOTIFY_CLIENT_SECRET";
        private static readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<SpotifyBot>().Build();

        public TaskCompletionSource<bool> Ready { get; }

        public delegate Task<Dictionary<ulong, string>> GetConnectionsEventHandler(ulong guildID);
        public event GetConnectionsEventHandler GetConnections;

        public SpotifyBot()
        {
            Ready = new();
        }

        public void StartClient()
        {
            Ready.SetResult(true);
        }

        public async Task<(string, IEnumerable<ICard>)> Fetch(ulong guildID, int count = 5)
        {
            await Ready.Task;
            var client = await GetClient();

            var songs = new Dictionary<string, ulong[]>();
            var songsWithRepeats = new List<KeyValuePair<string, ulong>>();
            foreach (var (id, conn) in await GetConnections(guildID))
            {
                var playlists = await client.Playlists.GetUsers(conn);
                foreach (var playlist in await client.PaginateAll(playlists))
                {
                    var tracks = await client.Playlists.GetItems(playlist.Id);
                    foreach (var track in await client.PaginateAll(tracks))
                    {
                        if (track.Track.ReadProperty("ExternalUrls") is Dictionary<string, string> url)
                        {
                            var keyValuePair = new KeyValuePair<string, ulong>(url.GetValueOrDefault("spotify"), id);
                            if (keyValuePair.Key != null) songsWithRepeats.Add(keyValuePair);
                        }
                    }
                }
            }

            foreach (var keyValuePair in songsWithRepeats)
            {
                if (songs.ContainsKey(keyValuePair.Key) && !songs[keyValuePair.Key].Contains(keyValuePair.Value))
                    songs[keyValuePair.Key] = songs[keyValuePair.Key].Concat(new ulong[] { keyValuePair.Value }).ToArray();
                else songs[keyValuePair.Key] = new ulong[] { keyValuePair.Value };
            }

            return ("Music", songs.Select(song => new Song(song.Key, song.Value))
                                    .Shuffle()
                                    .Take(count));
            
        }

        private static async Task<SpotifyClient> GetClient()
        {
            var config = SpotifyClientConfig.CreateDefault();
            var clientID = _config[SPOTIFY_CLIENT_ID]
                ?? throw new InvalidOperationException($"{SPOTIFY_CLIENT_ID} was not found in user secrets.");
            var clientSecret = _config[SPOTIFY_CLIENT_SECRET]
                ?? throw new InvalidOperationException($"{SPOTIFY_CLIENT_SECRET} was not found in user secrets.");
            var tokenResponse = await new OAuthClient(config)
                .RequestToken(new ClientCredentialsRequest(clientID, clientSecret));

            return new SpotifyClient(tokenResponse.AccessToken);
        }
    }

    public class Song : ICard
    {
        public string Question => $"<iframe style=\"border-radius:12px\" src=\"https://open.spotify.com/embed/track/{ID}?utm_source=generator\" width=\"100%\" height=\"152\" frameBorder=\"0\" allowfullscreen=\"\" allow=\"autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture\" loading=\"lazy\" </iframe>"; //"https://open.spotify.com/embed/track/7mfMTQ21RSVhUw778ymlyV?utm_source=generator"
        public string Answer => Users.Length.ToString();

        private string URL { get; set; }
        private string ID { get; set; }
        private ulong[] Users { get; set; }

        public Song(string url, ulong[] users)
        {
            URL = url;
            ID = new Uri(url).Segments.LastOrDefault();
            Users = users;
        }
    }
}
