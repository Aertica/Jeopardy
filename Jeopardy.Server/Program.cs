
using EmbedIO;
using Jeopardy.Bots;
using Microsoft.AspNetCore.Authentication.Cookies;
using NuGet.Packaging;
using static System.Net.WebRequestMethods;

namespace Jeopardy.Server
{
    public class Program
    {
        private const string DISCORD_CLIENT_ID = "DISCORD_CLIENT_ID";
        private const string DISCORD_CLIENT_SECRET = "DISCORD_CLIENT_SECRET";
        private static readonly IConfigurationRoot _config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        public static void Main(string[] args)
        {
            var bots = IBot.InitializeBots();

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
        }
    }
}
