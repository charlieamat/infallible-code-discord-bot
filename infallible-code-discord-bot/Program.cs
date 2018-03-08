using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            _client.Ready += ClientReady;

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task ClientReady()
        {
            return Task.Run(() => UpdateRulesIntervalAsync());
        }

        public async void UpdateRulesIntervalAsync()
        {
            while(true)
            {
                var rulesChannelId = ulong.Parse(GetConfigurationValue("RULES_CHANNEL_ID"));
                var rulesMessageId = ulong.Parse(GetConfigurationValue("RULES_MESSAGE_ID"));
                var embed = GenerateRulesEmbed();

                var channel = _client.GetChannel(rulesChannelId) as SocketTextChannel;
                if (channel == null)
                {
                    Console.WriteLine($"Unable to locate channel (id:{rulesChannelId})");
                    break;
                }
                else
                {
                    var message = await channel.GetMessageAsync(rulesMessageId);
                    if (message == null)
                    {
                        message = await channel.SendMessageAsync(string.Empty, embed: embed);
                    } else
                    {
                        await ((RestUserMessage) message).ModifyAsync(x => x.Embed = embed);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(DefaultUpdateInterval)).ConfigureAwait(false);
            }
        }

        public string GetConfigurationValue(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Please add `{DefaultEnvironmentVariablePrex}{key}` to your environment variables.");
            }
            return value;
        }

        public Embed GenerateRulesEmbed() =>
            new EmbedBuilder
            {
                Color = Color.Green,
                Title = "Welcome to the Infallible Code Discord!",
                Description = "We're a community of Unity developers helping each other become better programmers.",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Rules",
                        IsInline = true,
                        Value = "**1.** Respect everyone. Be polite.\n"
                        + "**2.** No text/reaction spam.\n"
                        + "**3.** No ALL CAPS messages.\n"
                        + "**4.** No text walls. Use Pastepin.\n"
                        + "**5.** No gore/porn/racism"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Have a Question?",
                        IsInline = true,
                        Value = "• Post in the appropriate channel\n"
                        + "• Be specific. Provide code (Gist/Pastebin)\n"
                        + "• No begging. We'll do our best to help you.\n"
                        + "• Search before posting."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Links",
                        IsInline = false,
                        Value = "YouTube: https://youtube.com/infalliblecode\n"
                        + "Twitch: http://twitch.tv/infalliblecode\n"
                        + "Patreon: http://patreon.com/infalliblecode\n"
                        + "Twitter: http://twitter.com/infalliblecode\n"
                        + "Facebook: http://facebook.com/infalliblecode\n"
                    },
                }
            }.Build();

        public Task Log(LogMessage message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}