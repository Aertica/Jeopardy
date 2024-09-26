using Discord.WebSocket;

namespace Jeopardy.Discord
{
    public partial class DiscordBot
    {
        private const string USER_AUTH_URI = "https://discord.com/oauth2/authorize?client_id=1108572849284853781&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A4000%2Fapi%2Foauth%2Fdiscord%2Fredirect&scope=identify+connections";
        private const string SERVER_AUTH_URI = "https://discord.com/oauth2/authorize?client_id=1108572849284853781&permissions=328565255168&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A4000%2Fapi%2Foauth%2Fdiscord%2Fredirect&integration_type=0&scope=identify+bot+connections";

        public async Task SlashCommandHandler(SocketSlashCommand command)
        {
            await command.DeferAsync();
            try
            {
                string result = command.CommandName switch
                {
                    nameof(authorize) => authorize(),
                    nameof(play) => play(command),
                    _ => throw new ArgumentException($"Command \"{command.CommandName}\" is not supported.", nameof(command)),
                };
                await command.FollowupAsync(result);
            }
            catch (Exception ex)
            {
                await command.FollowupAsync($"```{ex}```");
            }
        }

        private static string authorize()
        {
            return $"[Authorize]({USER_AUTH_URI}) Jeopardy Bot.";
                 //$"[Authorize]({SERVER_AUTH_URI}) Jeopardy Bot.";
        }

        private string play(SocketSlashCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
