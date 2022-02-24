using Discord.Commands;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuillBot {
    public static class Global {
        //Global variables used accessible across the entire program.
        public static Boolean TESTBOOL = false;
        public static Boolean LeagueBas = false;
        public static Dictionary<String, Boolean> ServerSearchList = new Dictionary<String, Boolean>{};
        public static System.Collections.Generic.IReadOnlyCollection<Discord.WebSocket.SocketGuild> GuildList;
        public static String DBLocation = @"URI=file:config/QuillBot.db";

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