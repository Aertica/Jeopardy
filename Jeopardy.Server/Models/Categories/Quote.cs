using Discord;
using Jeopardy.Discord;
using System.Text.RegularExpressions;

namespace Jeopardy.Server.Models
{
    public partial class Quote : BaseQuestion
    {
        #region IQuestion Members

        public override string Question => Text ?? string.Empty;
        public override string Answer => User ?? string.Empty;

        #endregion

        private string? Text { get; }
        private string? User { get; }
        public bool IsValid { get; private set; }

        public Quote(string quote)
        {
            IsValid = true;
            Text = ParseText(quote);
            User = ParseUser(quote);
        }

        //public Quote(string text, string user)
        //{
        //
        //}

        [GeneratedRegex("\"([^\"]*)\"")]
        private static partial Regex QuoteRegex();

        private string? ParseText(string quote)
        {
            try
            {
                return QuoteRegex().Matches(quote).First().Value;
            }
            catch (Exception)
            {
                IsValid = false;
                return null;
            }
        }

        private string? ParseUser(string quote)
        {
            try
            {
                int index = quote.LastIndexOf('-');
                return quote[index..][1..].Trim();
            }
            catch (Exception)
            {
                IsValid = false;
                return null;
            }
        }

        public override string ToString()
        {
            return $"{Text} - {User}";
        }

    }

    public static class QuoteExtensions
    {
        public static async Task<IEnumerable<Quote>> FetchQuotes(this DiscordBot bot, ulong guildID, int count = 5)
        {
            await bot.Ready.Task;

            var quotes = new List<IMessage>();
            var quotesChannel = bot.GetQuotesChannel(guildID);
            if (quotesChannel != null)
            {
                quotes.AddRange(await quotesChannel.GetMessagesAsync().FlattenAsync());

                var newQuotes = await quotesChannel.GetMessagesAsync(quotes.Last(), Direction.Before).FlattenAsync();
                while (newQuotes.Any())
                {
                    quotes.AddRange(newQuotes);
                    newQuotes = await quotesChannel.GetMessagesAsync(quotes.Last(), Direction.Before).FlattenAsync();
                }
            }

            return quotes.Select(quote => new Quote(quote.CleanContent))
                         .Where(quote => quote.IsValid)
                         .OrderBy(quote => new Random().Next())
                         .Take(count);
        }
    }
}
