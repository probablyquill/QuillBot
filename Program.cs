using System;
using System.Threading.Tasks;
using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

namespace QuillBot {
    class QuillBot {
        public static Task Main(string[] args) => new QuillBot().MainAsync();
        public Boolean TrackingToggleTest = true;

        public async Task MainAsync() {
            //CancellationTokenSource tokenSource = new CancellationTokenSource();
            //Task timerTask = FiveMinuteDelay(test, TimeSpan.FromMinutes(5), tokenSource.Token);
            
            var _client = new DiscordSocketClient();
            var _commands = new CommandService();
            CommandHandler commandHandler = new CommandHandler(_client, _commands);
            _client.Log += Log;

            //Remove API key before committing, change to load from file?
            String token = "";

            //Log the bot in
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await commandHandler.InstallCommandsAsync();

            //Wait indefinitely
            await Task.Delay(-1);
            
        }

        private Task Log(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private async Task FiveMinuteDelay(Action action, TimeSpan interval, CancellationToken token) {
            while (true) {
                action();
                await Task.Delay(interval, token);
            }
        }
    }
}