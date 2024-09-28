using Discord;
using Jeopardy.Discord;
using Jeopardy.Util;
using Jeopardy.Util.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.ObjectModel;

namespace Jeopardy.Server.Models
{
    public class GameBoard : Dictionary<string, IEnumerable<IQuestion>>
    {
        private const int NUM_CATEGORIES = 6;
        private const int NUM_QUESTIONS_PER_CATEGORY = 5;
        private static readonly string Path = $"{Environment.CurrentDirectory}\\ActiveGames.json";

        public Guid ID { get; }

        public GameBoard() // A 0 argument constructor is needed for json serialization.
        {
            ID = Guid.NewGuid();
        }

        public GameBoard(Guid id)
        {
            ID = id;

            using var reader = new JsonTextReader(File.OpenText(Path));
            var serializer = new JsonSerializer()
            {
                Converters = { new InterfaceConverter<IQuestion, BaseQuestion>() },
                Formatting = Formatting.Indented,
            };
            var activeGames = serializer.Deserialize<Dictionary<Guid, GameBoard>>(reader)
                ?? throw new JsonReaderException($"Error reading data at {Path}.");
            var gameboard = activeGames[ID]
                ?? throw new KeyNotFoundException($"Gameboard not found. ({ID})");
            foreach (var item in gameboard)
                Add( item.Key, item.Value);
        }

        private static string[] GetCategories(int count = 5)
        {
            var categories = new string[] { nameof(Quote) };
            return categories.OrderBy(category => new Random().Next()).Take(count).ToArray();
        }

        private async Task<IEnumerable<IQuestion>> GetQuestions(DiscordBot bot, ulong guildID, string category)
        {
            return await new Dictionary<string, Func<Task<IEnumerable<IQuestion>>>>()
            {
                { nameof(Quote), async () => await bot.FetchQuotes(guildID) },
            }[category]();
        }

        private void Save()
        {
            var activeGames = new Dictionary<Guid, GameBoard>();
            try
            {
                using var reader = new JsonTextReader(File.OpenText(Path));
                var serializer = new JsonSerializer()
                {
                    Converters = { new InterfaceConverter<IQuestion, BaseQuestion>() },
                    Formatting = Formatting.Indented,
                };
                activeGames = serializer.Deserialize<Dictionary<Guid, GameBoard>>(reader)
                    ?? throw new JsonReaderException($"Error reading data at {Path}.");
            }
            catch (Exception)
            {
                // If reading the active games failed, just overwrite the file.
            }
            finally
            {
                activeGames.TryAdd(ID, this);
                using var writer = new JsonTextWriter(File.CreateText(Path));
                var serializer = new JsonSerializer()
                {
                    Converters = { new InterfaceConverter<IQuestion, BaseQuestion>() },
                    Formatting = Formatting.Indented,
                };
                serializer.Serialize(writer, activeGames);
            }
        }

        public async Task Reset(DiscordBot bot, ulong guildID)
        {
            Clear();
            foreach (var category in GetCategories())
                TryAdd(category, await GetQuestions(bot, guildID, category));
            while (Count < NUM_CATEGORIES)
                TryAdd($"Category {Count + 1}", BaseQuestion.FetchQuestions());

            Save();
        }
    }
}
