
using Jeopardy.Discord;
using Jeopardy.Discord.OAuth;
using Jeopardy.Server.Controllers;
using Jeopardy.Server.Models;

namespace Jeopardy.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region Start the Discord Bot

            var bot = new DiscordBot();
            bot.StartClient();
            bot.OnPlay += async (ulong guildID) =>
            {
                GameBoard game = [];
                await game.Reset(bot, guildID);
                return game.ID;
            };

            using var server = new WebServer();
            server.Start();

            #endregion
            
            #region Start the Web App

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapFallbackToFile("/index.html");
            app.Run();

            #endregion
        }
    }
}
