using System;
using System.IO;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace QuillBot {
    class QuillBot {
        public static Task Main(string[] args) => new QuillBot().MainAsync();
        DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig(){AlwaysDownloadUsers = true, GatewayIntents = GatewayIntents.All});
        String DBLocation = @"URI=file:config/QuillBot.db";
        List<ulong> WarnedUIDs = new List<ulong>{};
        public async Task MainAsync() {
            //Load API auth Token from file.
            String token = loadToken();

            if (token == "" || token == "[ENTER BOT TOKEN HERE]\n") {
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
            Task timerTask = WaitFirst(PollUserStatus, TimeSpan.FromMinutes(.25), tokenSource.Token);
            //CancellationTokenSource leagueBanToken = new CancellationTokenSource();
            //Task leagueBanTask = GetGuilds(LeagueBanCheck, TimeSpan.FromMinutes(.5), leagueBanToken.Token);
            //SQLiteTesting();
            //Create backup every ten minutes.
            //Task backupTask = WaitFirst(CreateDBBackup, TimeSpan.FromMinutes(10), tokenSource.Token);
            //Wait indefinitely
            await Task.Delay(-1);
        }

        //Idk
        private Task Log(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        //Calls a method, then waits x amount of time after it completes to do it again.
        private async Task WaitAfter(Action action, TimeSpan interval, CancellationToken token) {
            while (true) {
                action();
                await Task.Delay(interval, token);
            }
        }

        //Waits x amount of time, then calls the method.
        private async Task WaitFirst(Action action, TimeSpan interval, CancellationToken token) {
            while (true) {
                await Task.Delay(interval, token);
                action();
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

        //Sets and outputs the GuildInfo in the Global class used for global data storage and access.
        private void CollectGuildInfo() {
            System.Collections.Generic.IReadOnlyCollection<Discord.WebSocket.SocketGuild> temp = _client.Guilds;
            Global.GuildList = temp;
            Global.ListGuilds();
            Global.UpdateDict();
        }

        //Joke league ban method (only sends messages in my server as-is.)
        private void LeagueBanCheck() {
            foreach (var guild in _client.Guilds) {
                foreach(var user in guild.Users) {
                    Boolean LeagueFound = false;

                    foreach (var activity in user.Activities) {
                        if (activity.Name.ToLower() == "league of legends") {
                            LeagueFound = true;
                            if (WarnedUIDs.Contains(user.Id)) {
                                Console.WriteLine("Banned " + user.DisplayName);
                                guild.GetTextChannel(826876630147923999).SendMessageAsync(user.DisplayName + ", You were warned, begone.");
                            } else {
                                WarnedUIDs.Add(user.Id);
                                Console.WriteLine("Warning " + user.DisplayName);
                                guild.GetTextChannel(826876630147923999).SendMessageAsync(user.DisplayName + ", you have 30 seconds to get and stay off before you're banned for playing league.");
                            }   
                        }
                    }

                    if (!LeagueFound) {
                        if (WarnedUIDs.Contains(user.Id)) {
                            WarnedUIDs.Remove(user.Id);
                        }
                    }
                }
            }
        }

        //Polls Status of users and keeps a list of servers for whitelist and toggle status.        
        private void PollUserStatus() {
            List<ulong> FinishedUsers = new List<ulong>{};
            var con = new SQLiteConnection(DBLocation);
            SQLiteDataReader response;
            int linesChanged = 0;
            con.Open();

            var cmd = new SQLiteCommand(con);
            //Create users table.
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS users(id INTEGER PRIMARY KEY, userid INTEGER, online INTEGER, offline INTEGER, started INTEGER, trackingstatus INTEGER, UNIQUE(userid))";
            cmd.ExecuteNonQuery();

            //Set up for later channel whitelisting / blacklisting
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS togglelist(id INTEGER PRIMARY KEY, serverid INTEGER, toggled INTEGER, whitelist INTEGER, UNIQUE(serverid))";
            cmd.ExecuteNonQuery();
            
            //Database Formats:
            //Users:
            // ID || User | Online | Offline | TimeTrackingStarted | Trackingstatus
            //    || INT  | INT    | INT     | INT                 | Boolean (INT 0 or 1)
            //
            //togglelist:
            // ID || serverid      | toggled | whitelist
            //    || INT           | BOOL    | BOOL
            //
            //id(serverID):
            // ID || channelid     | enabled
            //    || INT           | BOOL

            foreach(var guilds in _client.Guilds) {
                //SQL gets upset if the table name is just a number, so the number is concatonated onto the id.
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS id" + guilds.Id + "(id INTEGER PRIMARY KEY, channelid INTEGER, enabled INTEGER, whitelist INTEGER, UNIQUE(channelid))";
                cmd.ExecuteNonQuery();

                //Check if the guild exists in the toggle database.
                cmd.CommandText = "SELECT * FROM togglelist WHERE serverid = " + guilds.Id;
                response = cmd.ExecuteReader();

                //Will be used to store the value of toggled, since it is set to one when the entry is created, it is also assumed 1 here.
                int canrun = 1;

                //If there is no entry associated with the current server, make one. Otherwise, check tracking status.
                if (!response.HasRows) {
                    response.Close();
                    cmd.CommandText = "INSERT or IGNORE INTO togglelist(serverid, toggled, whitelist) VALUES(@serverid, @toggled, @whitelist)";
                    cmd.Parameters.AddWithValue("@serverid", guilds.Id);
                    cmd.Parameters.AddWithValue("@toggled", 1); //Enabled set to ON
                    cmd.Parameters.AddWithValue("@whitelist", 0); //Whitelist set to OFF
                    cmd.ExecuteNonQuery();
                } else {
                    response.Read();
                    canrun = response.GetInt32(2);
                    response.Close();
                }
                
                //Check to see if server tracking is disabled.
                if (canrun != 0) {
                    foreach(var user in guilds.Users) {
                        if (!FinishedUsers.Contains(user.Id)) {
                            cmd.CommandText = "SELECT * FROM users WHERE userid = " + user.Id;
                            response = cmd.ExecuteReader();

                            if (response.HasRows) {
                                response.Read();
                                //Save found information to variables
                                int rowID = response.GetInt32(0);
                                int userID = response.GetInt32(1);
                                int onlineCount = response.GetInt32(2);
                                int offlineCount = response.GetInt32(3);
                                int started = response.GetInt32(4);
                                int trackingStatus = response.GetInt32(5);
                                response.Close();

                                if (trackingStatus != 0) {
                                    //Update the online/offline element in the database based on the user's current status.
                                    cmd.CommandText = "";
                                    if (user.Status.ToString().ToLower() != "offline") {
                                        onlineCount += 1;
                                        cmd.CommandText = "UPDATE users SET online = " + onlineCount + " WHERE id = " + rowID;
                                    } else {
                                        offlineCount += 1;
                                        cmd.CommandText = "UPDATE users SET offline = " + offlineCount + " WHERE id = " + rowID;
                                    }
                                    //Check number of lines saved (testing variable)
                                    linesChanged = cmd.ExecuteNonQuery();
                                }
                            } else {
                                //Create new entry in database for user.
                                response.Close();
                                cmd.CommandText = "INSERT or IGNORE INTO users(userid, online, offline, started, trackingstatus) VALUES(@userid, @online, @offline, @started, @trackingstatus)";
                                cmd.Parameters.AddWithValue("@userid", user.Id);
                                if (user.Status.ToString().ToLower() != "offline") {
                                    cmd.Parameters.AddWithValue("@online", 1);
                                    cmd.Parameters.AddWithValue("@offline", 0);
                                } else {
                                    cmd.Parameters.AddWithValue("@online", 0);
                                    cmd.Parameters.AddWithValue("@offline", 1);
                                }
                                cmd.Parameters.AddWithValue("@started", 0);
                                cmd.Parameters.AddWithValue("@trackingstatus", 1);
                                linesChanged = cmd.ExecuteNonQuery();
                            }
                            FinishedUsers.Add(user.Id);
                        }
                    }
                }
            }
            con.Close();
        }
        private void SQLiteTesting() {
             //TODO: Save statuses to db
            //Format:
            // Users | Online | Offline | TimeTrackingStarted | Trackingstatus
            var con = new SQLiteConnection(Global.DBLocation);
            SQLiteDataReader response;
            //String output = "";
            con.Open();

            var cmd = new SQLiteCommand(con);

            //INSERT CODE TO TEST BELOW HERE
            //
            //

        }

        private void PrintDatabase(SQLiteCommand cmd) {
            cmd.CommandText = "SELECT * FROM users";
            SQLiteDataReader response = cmd.ExecuteReader();
            
            //Iterate through the list of found responses 
            while(response.Read()) {
                Console.WriteLine("ID\tUser\t\t\tOnline\tOffline\tStarted\tStatus");
                Console.WriteLine(response.GetInt32(0) + "\t" + response.GetInt64(1) + "\t" + response.GetInt32(2) + "\t" + response.GetInt32(3) + "\t" + response.GetInt32(4) +  "\t" + response.GetInt32(5));
            }

            response.Close();
        }

        private void CreateDBBackup() {
            File.Copy(Global.DBLocation, Global.DBLocation + ".bak");
        }
    }
}