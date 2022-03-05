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
        //ServerToggleDict has an array of integers stored ith the server long as they key.
        //The array of ints is sorted as [toggled, whitelist], with 0 being false and 1 being true.
        public static Dictionary<long, int[]> ServerToggleDict = new Dictionary<long, int[]>{};
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
            Dictionary<long, int[]> LoadedDict = new Dictionary<long, int[]>{};
            var con = new SQLiteConnection(DBLocation);
            SQLiteDataReader response;
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM togglelist";
            response = cmd.ExecuteReader();

            while(response.Read()) {
                //Retrieves server ID. the SQL query will return all servers where whitelisting is enabled, 
                int[] DictArray = new int[]{response.GetInt32(2), response.GetInt32(3)};
                LoadedDict.Add(response.GetInt64(1), DictArray);
            }
            response.Close();
            con.Close();

            ServerToggleDict = new Dictionary<long, int[]>(LoadedDict);
        }

        //RemoveFromWhitelist and AddtoWhitelist could be combined into a single function
        //Add a second input variable telling it what to change whitelist to?
        public static int RemoveFromWhitelist(long serverid) {
            //Assumes that the table already exists.
            var con = new SQLiteConnection(DBLocation);
            int linesChanged;
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "UPDATE togglelist SET whitelist = 0 WHERE serverid = " + serverid;
            linesChanged = cmd.ExecuteNonQuery();
            //Creates a new integer array using the old toggle value and the new whitelist value.
            ServerToggleDict[serverid] = new int[]{ServerToggleDict[serverid][0], 0};
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
            //Creates a new integer array using the old toggle value and the new whitelist value.
            ServerToggleDict[serverid] = new int[]{ServerToggleDict[serverid][0], 1};

            return linesChanged;
        }

        //EnableServerTracking and DisableServerTracking could be combined into a single function
        //Add a second input variable telling it what to change toggled to?
        public static int EnableServerTracking(long serverid) {
            var con = new SQLiteConnection(DBLocation);
            int linesChanged;
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "UPDATE togglelist SET toggled = 1 WHERE serverid =" + serverid;
            linesChanged = cmd.ExecuteNonQuery();
            ServerToggleDict[serverid] = new int[]{1, ServerToggleDict[serverid][1]};

            return linesChanged;
        }

        public static int DisableServerTracking(long serverid) {
            var con = new SQLiteConnection(DBLocation);
            int linesChanged;
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "UPDATE togglelist SET toggled = 0 WHERE serverid =" + serverid;
            linesChanged = cmd.ExecuteNonQuery();
            ServerToggleDict[serverid] = new int[]{0, ServerToggleDict[serverid][1]};

            return linesChanged;
        }
    }
}