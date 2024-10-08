﻿using Jeopardy.Util;
using Jeopardy.Util.Json;
using Newtonsoft.Json;

namespace Jeopardy.Bots
{
    public abstract class Bot
    {
        private static readonly string _path = $"{Environment.CurrentDirectory}\\ActiveGames.json";

        public abstract string Category { get; }
        public abstract TaskCompletionSource Ready { get; protected set; }
        public abstract void StartClient();
        public abstract Task StopClient();
        public abstract Task<IEnumerable<ICard>> FetchQuestions(ulong guildID, int count = 5);

        public static IEnumerable<Bot> InitializeBots()
        {
            List<Bot> bots = [];

            DiscordBot discordBot = new();
            discordBot.StartClient();
            discordBot.OnPlay += async (ulong guildID) =>
            {
                Guid id = Guid.NewGuid();
                var gameboard = new Dictionary<string, IEnumerable<ICard>>();
                foreach (var bot in bots.Shuffle().Take(6))
                {
                    var category = bot.Category;
                    var questions = await bot.FetchQuestions(guildID);
                    gameboard.Add(category, questions);
                }

                SaveGameBoard(id, gameboard);
                return id;
            };
            bots.Add(discordBot);

            SpotifyBot spotifyBot = new();
            spotifyBot.StartClient();
            spotifyBot.GetConnections += async (ulong guildID) =>
            {
                return await discordBot.GetConnections(guildID, DiscordBot.Connecttions.Spotify);
            };
            spotifyBot.GetUsername += async (ulong userID) =>
            {
                return await discordBot.GetUsername(userID);
            };
            bots.Add(spotifyBot);

            SteamBot steamBot = new();
            steamBot.StartClient();
            steamBot.GetConnections += async (ulong guildID) =>
            {
                return await discordBot.GetConnections(guildID, DiscordBot.Connecttions.Steam);
            };
            steamBot.GetUsername += async (ulong userID) =>
            {
                return await discordBot.GetUsername(userID);
            };
            bots.Add(steamBot);

            TriviaBot triviaBot = new();
            triviaBot.StartClient();
            bots.Add(triviaBot);

            return bots;
        }

        private static void SaveGameBoard(Guid id, Dictionary<string, IEnumerable<ICard>> gameboard)
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
