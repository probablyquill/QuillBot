using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace QuillBot {
    class QuillBot {
        public static Task Main(string[] args) => new QuillBot().MainAsync();
        private DiscordSocketClient _client;

        public async Task MainAsync() {
            _client = new DiscordSocketClient();
            _client.Log += Log;

            String token = "";

            //Log the bot in
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            //Wait indefinitely
            await Task.Delay(-1);
        }

        private Task Log(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}