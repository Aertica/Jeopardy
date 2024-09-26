using Discord;
using Discord.WebSocket;
using Jeopardy.Discord.OAuth;
using Microsoft.Extensions.Configuration;

namespace Jeopardy.Discord
{
    public partial class DiscordBot
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
            _ = new WebServer();
        }

        public void StartClient()
        {
            string token = _config[TOKEN]
                ?? throw new InvalidOperationException($"{TOKEN} was not found in user secrets.");

            Task.Run(async () =>
            {
                var clientReady = new TaskCompletionSource<bool>();
                await Client.LoginAsync(TokenType.Bot, token);
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
    }
}