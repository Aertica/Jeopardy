using Jeopardy.Util.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Jeopardy.Bots
{
    public class TriviaBot : Bot
    {
        private const string QUESTION_URI = "https://the-trivia-api.com/v2/questions";

        public override string Category => "Trivia";
        public override TaskCompletionSource Ready { get; protected set; }

        public TriviaBot()
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
            using var client = new HttpClient();
            var response = await client.GetAsync($"{QUESTION_URI}?limit={count}");
            string json = await response.Content.ReadAsStringAsync();
            var questions = JsonConvert.DeserializeObject<IEnumerable<TriviaQuestion>>(json)
                ?? throw new JsonReaderException($"Error reading data from {QUESTION_URI}."); ;

            return questions;
        }
    }

    /// <summary>
    /// API docs <see href="https://the-trivia-api.com/docs/v2/">here</see>.
    /// </summary>
    public class TriviaQuestion : ICard
    {
        [JsonIgnore]
        public string Question => question?["text"] ?? string.Empty;
        [JsonIgnore]
        public string Answer => correctAnswer ?? string.Empty;

        public TriviaQuestion() { }

        public string? id { get; set; }
        public string? category { get; set; }
        public string? difficulty { get; set; }
        public string? type { get; set; }
        public Dictionary<string, string>? question { get; set; }
        public string? correctAnswer { get; set; }
        public IEnumerable<string>? incorrectAnswers { get; set; }
        public IEnumerable<string>? tags { get; set; }
        public IEnumerable<string>? regions { get; set; }
        public bool? isNiche { get; set; }
    }
}
