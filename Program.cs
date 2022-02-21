using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace QuillBot {
    class QuillBot {
        public static Task Main(string[] args) => new QuillBot().MainAsync();
        public Boolean TrackingToggleTest = true;

        public async Task MainAsync() {
            //Load API auth Token from file.
            String token = loadToken();

            if (token == "") {
                Console.WriteLine("\nToken could not be read, check the token file at \"config/token\"");
                Console.WriteLine("If it does not exist please create a \"token\" file with no extension, and paste the token inside of it.\n");
                System.Environment.Exit(1);
            }

            //CancellationTokenSource tokenSource = new CancellationTokenSource();
            //Task timerTask = FiveMinuteDelay(test, TimeSpan.FromMinutes(5), tokenSource.Token);
            
            var _client = new DiscordSocketClient();
            var _commands = new CommandService();
            CommandHandler commandHandler = new CommandHandler(_client, _commands);
            _client.Log += Log;

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

        private String loadToken() {
            String token;
            String path = "config/token";
            try {
                FileStream fs = File.Open(path, FileMode.Open);
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);

                //Read the file, convert the raw bytes to a string, then remove any null characters.
                //Having the full length string from reading B results in ~1k '\0' characters being 
                //appended to the end.
                fs.Read(b,0,b.Length);
                token = temp.GetString(b);
                token = token.Trim('\0');
                fs.Close();

                return token;
            } catch {}
            
            return "";
        }
    }
}