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

        DiscordSocketClient _client = new DiscordSocketClient(); 

        public async Task MainAsync() {
            //Load API auth Token from file.
            String token = loadToken();

            if (token == "") {
                Console.WriteLine("\nToken could not be read, check the token file at \"config/token\"");
                Console.WriteLine("If it does not exist please create a \"token\" file with no extension, and paste the token inside of it.\n");
                System.Environment.Exit(1);
            }

            //Global.loadFromFile();
            
            //var _client = new DiscordSocketClient();
            var _commands = new CommandService();
            CommandHandler commandHandler = new CommandHandler(_client, _commands);
            _client.Log += Log;

            //Log the bot in
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await commandHandler.InstallCommandsAsync();

            //Generated a task and cancellation token which checks what servers the bot is in every X minutes.
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task timerTask = GetGuilds(CollectGuildInfo, TimeSpan.FromMinutes(5), tokenSource.Token);
            CancellationTokenSource leagueBanToken = new CancellationTokenSource();
            Task leagueBanTask = GetGuilds(LeagueBanCheck, TimeSpan.FromMinutes(5), tokenSource.Token);

            //Wait indefinitely
            await Task.Delay(-1);
            
        }

        //Idk
        private Task Log(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        //Template for having a piece of code execute once every X minutes after its last run finished.
        private async Task XMinuteDelay(Action action, TimeSpan interval, CancellationToken token) {
            while (true) {
                action();
                await Task.Delay(interval, token);
            }
        }

        //Load the discord bot api token from a file in the config folder.
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
                fs.Read(b, 0 ,b.Length);
                token = temp.GetString(b);
                token = token.Trim('\0');
                fs.Close();

                return token;
            } catch {}
            
            return "";
        }

        //Calls the CollectGuildInfo method every x minutes, specified by the initial Task.
        private async Task GetGuilds(Action action, TimeSpan interval, CancellationToken token) {
            while (true) {
                await Task.Delay(interval, token);
                action();
            }
        }

        //Sets and outputs the GuildInfo in the Global class used for global data storage and access.
        private void CollectGuildInfo() {
            System.Collections.Generic.IReadOnlyCollection<Discord.WebSocket.SocketGuild> temp = _client.Guilds;
            Global.GuildList = temp;
            Global.ListGuilds();
            Global.UpdateDict();
        }

        private void LeagueBanCheck() {
            foreach (var guild in _client.Guilds) {
                foreach(var user in guild.Users) {
                    if (user.Activities != null) {
                        Console.WriteLine("User: " + user + " is playing " + user.Activities);
                    }
                }
            }
        }
    }
}