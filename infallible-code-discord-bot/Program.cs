using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace InfallibleCode.DiscordBot
{
    public class Program
    {
        private IConfigurationRoot _configuration;
        private DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var discordToken = _configuration["DISCORD_TOKEN"];
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                throw new Exception($"Please enter your Discord bot's token into the `DISCORD_TOKEN` environment variable.");
            }

            _client = new DiscordSocketClient();
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            UpdateRulesInterval();

            await Task.Delay(-1);
        }

        public async void UpdateRulesInterval()
        {
            while(true)
            {
                var channels = await _client.GetGroupChannelsAsync();
                var target = channels.First(channel => channel.Name == "Rules");
                
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        public Task Log(LogMessage message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}