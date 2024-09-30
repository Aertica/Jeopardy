using Jeopardy.Bots;
using Jeopardy.Util.Json;
using Newtonsoft.Json;

namespace Jeopardy.Server.Models
{
    public class GameBoard : Dictionary<string, IEnumerable<ICard>>
    {
        private static readonly string Path = $"{Environment.CurrentDirectory}\\ActiveGames.json";

        public GameBoard() {} // A 0 argument constructor is needed for json serialization and deserialization.

        public GameBoard(Guid id)
        {
            using var reader = new JsonTextReader(File.OpenText(Path));
            var serializer = new JsonSerializer()
            {
                Converters = { new InterfaceConverter<ICard, Card>() },
                Formatting = Formatting.Indented,
            };
            var activeGames = serializer.Deserialize<Dictionary<Guid, GameBoard>>(reader)
                ?? throw new JsonReaderException($"Error reading data at {Path}.");
            var gameboard = activeGames[id]
                ?? throw new KeyNotFoundException($"Gameboard not found. ({id})");
            foreach (var item in gameboard)
                Add( item.Key, item.Value);
        }
    }
}
