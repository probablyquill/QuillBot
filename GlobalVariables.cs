using Discord.Commands;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Data.SQLite;

namespace QuillBot {
    public static class Global {
        //Global variables used accessible across the entire program.
        public static Boolean TESTBOOL = false;
        public static Boolean LeagueBas = false;
        public static Dictionary<String, Boolean> ServerSearchList = new Dictionary<String, Boolean>{};
        public static System.Collections.Generic.IReadOnlyCollection<Discord.WebSocket.SocketGuild> GuildList;
        public static List<long> WhitelistList = new List<long>{};
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

        public static void RetrieveWhitelist() {
            List<long> serversWithWhiteList = new List<long>{};
            var con = new SQLiteConnection(DBLocation);
            SQLiteDataReader response;
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM togglelist WHERE whitelist = 1";
            response = cmd.ExecuteReader();

            while(response.Read()) {
                //Retrieves server ID. the SQL query will return all servers where whitelisting is enabled, 
                serversWithWhiteList.Add(response.GetInt64(1));
            }
            response.Close();
            con.Close();

            WhitelistList = new List<long>(serversWithWhiteList);
            WhitelistList.Sort();
        }

        public static int RemoveFromWhitelist(long serverid) {
            //Assumes that the table already exists.
            var con = new SQLiteConnection(DBLocation);
            int linesChanged;
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "UPDATE togglelist SET whitelist = 0 WHERE serverid = " + serverid;
            linesChanged = cmd.ExecuteNonQuery();
            WhitelistList.Remove(serverid);
            return linesChanged;
        }

        public static int AddToWhitelist(long serverid) {
            //Assumes that the table already exists.
            var con = new SQLiteConnection(DBLocation);
            int linesChanged;
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "UPDATE togglelist SET whitelist = 1 WHERE serverid = " + serverid;
            linesChanged = cmd.ExecuteNonQuery();
            WhitelistList.Add(serverid);

            return linesChanged;
        }
    }
}