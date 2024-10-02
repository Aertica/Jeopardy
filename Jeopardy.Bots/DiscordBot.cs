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
using Jeopardy.Bots.Exceptions;
using System.Reflection.Metadata.Ecma335;

namespace Jeopardy.Bots
{
    public partial class DiscordBot : Bot
    {
        private const string DISCORD_TOKEN = "DISCORD_TOKEN";
        private const string QUOTES_CHANNEL = "quotes";
        private readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<DiscordBot>().Build();

        public override string Category => "Quotes";
        public override TaskCompletionSource Ready { get; protected set; }
        private DiscordSocketClient Client { get; }

        public DiscordBot()
        {
            Ready = new();
            Client = new(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                MessageCacheSize = 32
            });
        }

        public override void StartClient()
        {
            Task.Run(async () =>
            {
                try
                {
                    string token = _config[DISCORD_TOKEN]
                        ?? throw new InvalidOperationException($"{DISCORD_TOKEN} was not found in user secrets.");
                    var clientReady = new TaskCompletionSource();
                    await Client.LoginAsync(Discord.TokenType.Bot, token);
                    await Client.StartAsync();
                    Client.Ready += () =>
                    {
                        clientReady.SetResult();
                        return Task.CompletedTask;
                    };
                    await clientReady.Task;

                    var commands = new ApplicationCommandProperties[3];

                    var authCommand = new SlashCommandBuilder();
                    authCommand.WithName(nameof(authorize));
                    authCommand.WithDescription("Authorize Jeopardy Bot to use your Discord information.");
                    commands[0] = authCommand.Build();

                    var inviteCommand = new SlashCommandBuilder();
                    inviteCommand.WithName(nameof(invite));
                    inviteCommand.WithDescription("Invite Jeopardy Bot to another server.");
                    commands[1] = inviteCommand.Build();

                    var playCommand = new SlashCommandBuilder();
                    playCommand.WithName(nameof(play));
                    playCommand.WithDescription("Start a game of Jeopardy using this server.");
                    commands[2] = playCommand.Build();

                    await Client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
                    Client.SlashCommandExecuted += SlashCommandHandler;
                    Ready.SetResult();

                    Client.MessageReceived += async (message) =>
                    {
                        if (message.Channel.Id == 1142257646569787413u)
                        {
                            var y = Emoji.Parse(":white_check_mark:");
                            var n = Emoji.Parse(":x:");
                            var quote = new Quote(message.CleanContent);
                            if (quote.IsValid)
                                await message.AddReactionAsync(y);
                            else
                                await message.AddReactionAsync(n);
                        }
                    };

                    Client.MessageUpdated += async (chachable, message, channel) =>
                    {
                        // For some reason the supplied message's content is empty.
                        var m = await ((SocketTextChannel)Client.GetChannel(channel.Id)).GetMessageAsync(message.Id);

                        if (m.Channel.Id == 1142257646569787413u)
                        {
                            var y = Emoji.Parse(":white_check_mark:");
                            var n = Emoji.Parse(":x:");
                            await m.RemoveReactionAsync(y, Client.CurrentUser);
                            await m.RemoveReactionAsync(n, Client.CurrentUser);
                            var quote = new Quote(m.CleanContent);
                            if (quote.IsValid)
                                await m.AddReactionAsync(y);
                            else
                                await m.AddReactionAsync(n);
                        }
                    };

                    var info = await Client.GetApplicationInfoAsync();
                    var server = new AuthServer(info.RedirectUris);
                    server.Start();

                    await Task.Delay(-1);
                }
                catch (Exception ex)
                {
                    if (Client.LoginState == LoginState.LoggedIn)
                        await Client.LogoutAsync();

                    Ready.SetException(ex);
                }
            });
        }
        
        public override async Task StopClient()
        {
            await Client.LogoutAsync();
            Ready = new();
        }

        public override async Task<IEnumerable<ICard>> FetchQuestions(ulong guildID, int count = 5)
        {
            await Ready.Task;
            if (Ready.Task.Exception is not null)
                throw Ready.Task.Exception;

            var quotes = new List<IMessage>();
            var quotesChannel = await GetQuotesChannel(guildID);
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
                         .Shuffle()
                         .Take(count);
        }

        public async Task<string> GetUsername(ulong userID)
        {
            using var client = await new Token(userID).GetClient()
                ?? throw new UnauthorizedException(userID, $"User {userID} was not authorized.");

            return client.CurrentUser.Username;
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

        private async Task<SocketTextChannel?> GetQuotesChannel(ulong guildID)
        {
            await Ready.Task;
            return Client.GetGuild(guildID)?.Channels.Where(channel => channel.Name == QUOTES_CHANNEL).SingleOrDefault() as SocketTextChannel;
        }

        private static async Task<Dictionary<string, string>> GetUserConnections(ulong userID)
        {
            using var client = await new Token(userID).GetClient()
                ?? throw new UnauthorizedException(userID, $"User {userID} was not authorized.");

            var connections = new Dictionary<string, string>();
            foreach (IConnection connection in await client.GetConnectionsAsync())
                connections.Add(connection.Type, connection.Id);

            return connections;
        }

        public static class Connecttions
        {
            public const string Spotify = "spotify";
            public const string Steam = "steam";
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