using Jeopardy.Util;
using Microsoft.Extensions.Configuration;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using Swan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Bots
{
    public class SteamBot : Bot
    {
        private const string STEAM_KEY = "STEAM_KEY";
        private readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<DiscordBot>().Build();

        public delegate Task<Dictionary<ulong, string>> GetConnectionsEventHandler(ulong guildID);
        public event GetConnectionsEventHandler? GetConnections;

        public delegate Task<string> GetUsernameEventHandler(ulong userID);
        public event GetUsernameEventHandler? GetUsername;

        public override string Category => "Steam";
        public override TaskCompletionSource Ready { get; protected set; }

        public SteamBot()
        {
            Ready = new();
        }

        public override void StartClient()
        {
            Task.Run(() =>
            {
                try
                {
                    Ready.SetResult();
                }
                catch (Exception ex)
                {
                    Ready.SetException(ex);
                }
            });
        }

        public override Task StopClient()
        {
            Ready = new();
            return Task.CompletedTask;
        }

        public override async Task<IEnumerable<ICard>> FetchQuestions(ulong guildID, int count = 5)
        {
            var steamKey = _config[STEAM_KEY]
                ?? throw new InvalidOperationException($"{STEAM_KEY} was not found in user secrets.");
            var Factory = new SteamWebInterfaceFactory(steamKey);
            var playerInterface = Factory.CreateSteamWebInterface<PlayerService>(new HttpClient());

            var games = new List<Game>();
            var gamesWithRepeats = new List<Game>();
            foreach (var (id, conn) in await GetConnections.Invoke(guildID))
            {
                var username = await GetUsername.Invoke(id);
                var userGames = (await playerInterface.GetOwnedGamesAsync(ulong.Parse(conn), true, true)).Data.OwnedGames;
                foreach (var userGame in userGames)
                {
                    var game = new Game(userGame.AppId, userGame.Name, username, userGame.PlaytimeForever);
                    gamesWithRepeats.Add(game);
                }
            }

            foreach (var game in gamesWithRepeats)
            {
                if (!games.All(g => g.Id != game.Id) && !games.Find(g => g.Id == game.Id).Equals(game))
                    games.Find(g => g.Id == game.Id).Add(game);
                else games.Add(game);
            }

            return games.Shuffle()
                        .Take(count);
        }
    }

        public partial class Game : ICard
        {
            public string Question => Name;
            public string Answer => UserPlayTimes.Humanize();

            public Game(ulong id, string name, string username, TimeSpan playTime)
            {
                Id = id;
                Name = name;
                Users = [username];
                UserPlayTimes = [playTime];
            }

            public ulong Id { get; }
            public string Name { get; }
            public string[] Users { get; set; }
            public TimeSpan[] UserPlayTimes { get; set; }

            public void Add(Game game)
            {
                Users = Users.Concat(game.Users).ToArray();
                UserPlayTimes = UserPlayTimes.Concat(game.UserPlayTimes).ToArray();
            }

            public bool Equals(Game game)
            {
                return new HashSet<string>(Users).SetEquals(game.Users);
            }
        }
    
}
