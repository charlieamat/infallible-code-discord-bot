using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace InfallibleCode.DiscordBot
{
    public class Program
    {
        private const int DefaultUpdateInterval = 3;
        private const string DefaultEnvironmentVariablePrex = "ICDB_";

        private IConfigurationRoot _configuration;
        private DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables(DefaultEnvironmentVariablePrex)
                .Build();

            var discordToken = GetConfigurationValue("DISCORD_TOKEN");

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
                var rulesChannelId = ulong.Parse(GetConfigurationValue("RULES_CHANNEL_ID"));
                var rulesMessageId = ulong.Parse(GetConfigurationValue("RULES_MESSAGE_ID")); 

                var channel = _client.GetChannel(rulesChannelId) as SocketTextChannel;
                if (channel == null)
                {
                    Console.WriteLine($"Unable to locate channel (id:{rulesChannelId})");
                    break;
                } else
                {
                    string action;
                    var message = await channel.GetMessageAsync(rulesMessageId);
                    if (message == null)
                    {
                        action = "Posted";
                        message = await channel.SendMessageAsync(action);
                    } else
                    {
                        action = "Updated";
                        await ((RestUserMessage) message).ModifyAsync(x => x.Content = action);
                    }
                    Console.WriteLine($"{action} rules message (id:{rulesMessageId}) in {channel.Name} (id:{message.Id})");
                }
                await Task.Delay(TimeSpan.FromSeconds(DefaultUpdateInterval));
            }
        }

        public string GetConfigurationValue(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Please enter a value for `{DefaultEnvironmentVariablePrex}{key}` in your environment variable.");
            }
            return value;
        }

        public Task Log(LogMessage message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}