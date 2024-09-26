
using Jeopardy.Discord;

namespace Jeopardy.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region Start the Discord Bot

            var bot = new DiscordBot();
            bot.StartClient();

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
