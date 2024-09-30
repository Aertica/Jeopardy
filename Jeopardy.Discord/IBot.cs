using Jeopardy.Util;
using Jeopardy.Util.Json;
using Newtonsoft.Json;

namespace Jeopardy.Bots
{
    public interface IBot
    {
        public abstract TaskCompletionSource<bool> Ready { get; }
        public abstract void StartClient();
        public abstract Task<(string, IEnumerable<ICard>)> Fetch(ulong guildID, int count = 5);

        public static sealed IEnumerable<IBot> InitializeBots()
        {
            List<IBot> bots = [];

            DiscordBot discordBot = new();
            discordBot.StartClient();
            discordBot.OnPlay += async (ulong guildID) =>
            {
                await discordBot.Ready.Task;

                Guid id = Guid.NewGuid();
                var gameboard = new Dictionary<string, IEnumerable<ICard>>();
                foreach (var bot in bots.Shuffle().Take(6))
                {
                    var (name, questions) = await bot.Fetch(guildID);
                    gameboard.Add(name, questions);
                }

                SaveGameBoard(id, gameboard);
                return id;
            };
            bots.Add(discordBot);

            SpotifyBot spotifyBot = new();
            spotifyBot.StartClient();
            spotifyBot.GetConnections += async (ulong guildID) =>
            {
                await spotifyBot.Ready.Task;
                return await discordBot.GetConnections(guildID, DiscordBot.Connecttions.Spotify);
            };
            bots.Add(spotifyBot);

            return bots;
        }

        private static readonly string _path = $"{Environment.CurrentDirectory}\\ActiveGames.json";

        private static sealed void SaveGameBoard(Guid id, Dictionary<string, IEnumerable<ICard>> gameboard)
        {
            var activeGames = new Dictionary<Guid, Dictionary<string, IEnumerable<ICard>>>();
            try
            {
                using var reader = new JsonTextReader(File.OpenText(_path));
                var serializer = new JsonSerializer()
                {
                    Converters = { new InterfaceConverter<ICard, Card>() },
                    Formatting = Formatting.Indented,
                };
                activeGames = serializer.Deserialize<Dictionary<Guid, Dictionary<string, IEnumerable<ICard>>>>(reader)
                    ?? throw new JsonReaderException($"Error reading data at {_path}.");
            }
            catch (Exception)
            {
                // If reading the active games failed, just overwrite the file.
            }
            finally
            {
                activeGames.TryAdd(id, gameboard);
                using var writer = new JsonTextWriter(File.CreateText(_path));
                var serializer = new JsonSerializer()
                {
                    Converters = { new InterfaceConverter<ICard, Card>() },
                    Formatting = Formatting.Indented,
                };
                serializer.Serialize(writer, activeGames);
            }
        }
    }

    public interface ICard
    {
        public string Question { get; }
        public string Answer { get; }
    }

    public sealed class Card : ICard
    {
        public string Question { get; private set; } = "Question";
        public string Answer { get; private set; } = "Answer";
    }
}
