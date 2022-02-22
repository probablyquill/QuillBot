using Discord.Commands;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuillBot {
    public static class Global {
        public static Boolean TESTBOOL = false;
        public static Dictionary<String, String> ServerSearchList = new Dictionary<String, String>{};

        public static System.Collections.Generic.IReadOnlyCollection<Discord.WebSocket.SocketGuild> GuildList;
    
        //Load list of servers where user online percentage has been enabled.
        public static void loadFromFile() {
            if(!File.Exists("config/enabledServers.json")) File.Create("config/enabledServers.json");
            String enabledServers = File.ReadAllText("config/enabledServers.json");
            //if(enabledServers != "") ServerSearchList = JsonSerializer.Deserialize<Dictionary<String, String>>(enabledServers);

            Console.WriteLine(ServerSearchList);
        }

        //Prints out a list of the guilds which the bot is in.
        public static void ListGuilds() {
            foreach(var guild in GuildList) {
                Console.WriteLine(guild);
            }
        }
    }
}