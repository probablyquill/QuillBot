using Discord.Commands;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuillBot {
    public static class Global {
        public static Boolean TESTBOOL = false;
        public static Boolean LeagueBas = false;
        public static Dictionary<String, Boolean> ServerSearchList = new Dictionary<String, Boolean>{};
        public static System.Collections.Generic.IReadOnlyCollection<Discord.WebSocket.SocketGuild> GuildList;
    
        //Load list of servers where user online percentage has been enabled.
        public static void LoadFromFile() {
            if(!File.Exists("config/enabledServers.json")) File.Create("config/enabledServers.json");
            String enabledServers = File.ReadAllText("config/enabledServers.json");
            if(enabledServers != "") ServerSearchList = JsonSerializer.Deserialize<Dictionary<String, Boolean>>(enabledServers);
            //Come back to
            Console.WriteLine(ServerSearchList);
        }

        //Update the ID:Boolean
        //Funny one line loop (make readable later)
        public static void UpdateDict() {
            foreach (var guild in GuildList) if (!ServerSearchList.ContainsKey(guild.Id.ToString())) ServerSearchList.Add(guild.Id.ToString(), false); 
            }

        //Prints out a list of the guilds which the bot is in.
        public static void ListGuilds() {
            foreach(var guild in GuildList) {
                Console.WriteLine(guild.Id);
            }
        }
    }
}