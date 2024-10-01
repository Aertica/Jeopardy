using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Discord.Rest;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using EmbedIO;
using Jeopardy.Util;
using Jeopardy.Bots.OAuth;

namespace Jeopardy.Bots
{
    public partial class DiscordBot : IBot
    {
        private const string TOKEN = "TOKEN";
        private const string QUOTES_CHANNEL = "quotes";
        private readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<DiscordBot>().Build();

        public DiscordSocketClient Client { get; }
        public TaskCompletionSource<bool> Ready { get; }

        public DiscordBot()
        {
            Client = new(new() { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers });
            Ready = new();
        }

        public void StartClient()
        {
            string token = _config[TOKEN]
                ?? throw new InvalidOperationException($"{TOKEN} was not found in user secrets.");

            Task.Run(async () =>
            {
                var server = new WebServer();
                server.Start();

                var clientReady = new TaskCompletionSource<bool>();
                await Client.LoginAsync(Discord.TokenType.Bot, token);
                await Client.StartAsync();
                Client.Ready += () =>
                {
                    clientReady.SetResult(true);
                    return Task.CompletedTask;
                };
                await clientReady.Task;

                var commands = new List<ApplicationCommandProperties>();

                var authCommand = new SlashCommandBuilder();
                authCommand.WithName(nameof(authorize));
                authCommand.WithDescription("Authorize Jeopardy Bot to use your Discord information.");
                commands.Add(authCommand.Build());

                var inviteCommand = new SlashCommandBuilder();
                inviteCommand.WithName(nameof(invite));
                inviteCommand.WithDescription("Invite Jeopardy Bot to another server.");
                commands.Add(inviteCommand.Build());

                var playCommand = new SlashCommandBuilder();
                playCommand.WithName(nameof(play));
                playCommand.WithDescription("Start a game of Jeopardy using this server.");
                commands.Add(playCommand.Build());

                await Client.BulkOverwriteGlobalApplicationCommandsAsync(commands.ToArray());
                Client.SlashCommandExecuted += SlashCommandHandler;
                Ready.SetResult(true);

                await Task.Delay(-1);
            });
        }

        public SocketTextChannel? GetQuotesChannel(ulong guildID)
        {
            return Client.GetGuild(guildID)?.Channels.Where(channel => channel.Name == QUOTES_CHANNEL).SingleOrDefault() as SocketTextChannel;
        }

        public static async Task<Dictionary<string, string>> GetUserConnections(ulong userID)
        {
            using var client = await new Token(userID).GetClient();

            var connections = new Dictionary<string, string>();
            foreach (IConnection connection in await client.GetConnectionsAsync())
                connections.Add(connection.Type, connection.Id);

            return connections;
        }

        public async Task<Dictionary<ulong, string>> GetConnections(ulong guildID, string connection)
        {
            await Ready.Task;
            var connections = new Dictionary<ulong, string>();
            foreach (var user in Client.GetGuild(guildID).Users.Where(user => !user.IsBot))
            {
                var userConnections = await GetUserConnections(user.Id);
                connections.Add(user.Id, userConnections[connection]);
            }
        
            return connections;
        }

        public async Task<string> GetUsername(ulong userID)
        {
            using var client = await new Token(userID).GetClient();
            return client.CurrentUser.Username;
        }

        public static class Connecttions
        {
            public const string Spotify = "spotify";
            public const string Steam = "steam";
        }

        public async Task<(string, IEnumerable<ICard>)> Fetch(ulong guildID, int count = 5)
        {
            guildID = 700465481122840626u;
            await Ready.Task;

            var quotes = new List<IMessage>();
            var quotesChannel = GetQuotesChannel(guildID);
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

            return ("Quotes", quotes.Select(quote => new Quote(quote.CleanContent))
                                    .Where(quote => quote.IsValid)
                                    .Shuffle()
                                    .Take(count));
        }
    }

    public partial class Quote : ICard
    {
        #region IQuestion Members

        public string Question => Text ?? string.Empty;
        public string Answer => User ?? string.Empty;

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
}