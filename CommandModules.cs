using Discord.Commands;
using System.Data.SQLite;
using Discord.WebSocket;

namespace QuillBot {
    public class MainModule : ModuleBase<SocketCommandContext> {
        //Say Hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] String echo) => ReplyAsync(echo);

        //Toggle the user online tracking function.
        [Command("trackerToggle")]
        [Summary("Toggles the online percentage tracker on or off.")]
        public Task ToggleAsync() {

            //Toggle the state of the tracking variable. This will be changed to a per server basis.
            //Use JSON file and dictionaries to story servers to track?
            switch (Global.TESTBOOL) {
                case true:
                    Global.TESTBOOL = false;
                    break;
                default:
                    Global.TESTBOOL = true;
                    break;
            }

            String output = "TESTBOOL is now " + Global.TESTBOOL;
            return ReplyAsync(output);
        }
        
        //Info Command
        [Command("info")]
        [Summary("Displays command list.")]
        public Task ReturnInfo() {
            String output = 
                "__Commands__:\n\n**$say** => Have the bot echo whatever was typed after 'say'.\n\n**$trackerToggle** => Toggle online percentage tracking for this server (Not implemented).";
            output += "\n\n**$toggle** => Toggle whether the bot will track your online status. Default is on. This is not server specific.";
            output += "\n\n**$online** => Displays the percentage of time a user has been online. Can also be called at a user, such as $online @QuillBot";
            output += "\n\n**$info** => Displays this menu";
            output += "\n\n*Contact: quill@probablyquill.com*";
            return ReplyAsync(output);
        }
        //Display User Online Time
        [Command("online")]
        [Summary("Displays the percentage of time which the user has been online.")]
        public Task OnlineInfo(SocketUser user = null) {
            //Create database connection
            var con = new SQLiteConnection(Global.UserDBLocation);
            SQLiteDataReader response;
            String output = "";
            //int linesChanged;
            con.Open();

            //Connect to the database
            var cmd = new SQLiteCommand(con);

            //Select based on user's id
            if (user == null) {
                cmd.CommandText = "SELECT * FROM users WHERE userid = " + Context.User.Id; 
            } else {
                cmd.CommandText = "SELECT * FROM users WHERE userid = " + user.Id; 
            }
            response = cmd.ExecuteReader();

            if (response.HasRows) {
                response.Read();

                //Output Testing
                //Console.WriteLine("ID\tUser\t\t\tOnline\tOffline\tStarted\tStatus");
                //Console.WriteLine(response.GetInt32(0) + "\t" + response.GetInt64(1) + "\t" + response.GetInt32(2) + "\t" + response.GetInt32(3) + "\t" + response.GetInt32(4) +  "\t" + response.GetInt32(5));

                //Save variables based on user's returned information.
                int rowID = response.GetInt32(0);
                long userID = response.GetInt64(1);
                double onlineCount = response.GetInt32(2);
                double offlineCount = response.GetInt32(3);
                int started = response.GetInt32(4);
                int trackingStatus = response.GetInt32(5);
                response.Close();
                con.Close();

                if (trackingStatus != 0) {
                    //Calculate online percentage
                    double onlineTime = 100;
                    if (offlineCount != 0) {
                        onlineTime = onlineCount / (offlineCount + onlineCount);
                        onlineTime = onlineTime * 100;
                    }

                    double hourtotal = 15 * onlineCount;
                    hourtotal = hourtotal / (60 * 60);
                    hourtotal = hourtotal * 100;
                    int hourInt = (int) hourtotal;
                    hourtotal = (double) hourInt / 100;
                    //Message String
                    if (user == null) {
                        output += "You have been online " + (int) onlineTime + "% of the time since tracking started, or approximately " + hourtotal + " hours.";
                    } else {
                        output += "" + user.Username + " has been online " + (int) onlineTime + "% of the time since tracking started, or approximately " + hourtotal + " hours.";
                    }
                } else {
                    if (user != null) {
                        output = "Tracking is disabled for this user.";
                    } else {
                        output = "You have disabled tracking, enable it by using \"$toggle.\"";
                    }  
                }
            } else {
                output = "User not found, please wait 30s and try again.";
            }
            return ReplyAsync(output);
        }
        [Command("toggle")]
        [Summary("Toggles the tracking status of the user who called it.")]
        public Task ToggleTracking() {
            //Create database connection
            var con = new SQLiteConnection(Global.UserDBLocation);
            SQLiteDataReader response;
            //int linesChanged;
            con.Open();

            //Connect to the database
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT * FROM users WHERE userid = " + Context.User.Id; 
            response = cmd.ExecuteReader();

            //Check to see if there was a match in the database.
            if (response.HasRows) {
                //Save variables based on user's returned information.
                response.Read();
                int rowID = response.GetInt32(0);
                long userID = response.GetInt64(1);
                double onlineCount = response.GetInt32(2);
                double offlineCount = response.GetInt32(3);
                int started = response.GetInt32(4);
                int trackingStatus = response.GetInt32(5);
                response.Close();
                
                //Reverse tracking status
                switch(trackingStatus) {
                    case 0:
                        trackingStatus = 1;
                        break;
                    default:
                        trackingStatus = 0;
                        break;
                }

                //Update tracking for the user and close the database connection.
                cmd.CommandText = "UPDATE users SET trackingstatus = " + trackingStatus + " WHERE id = " + rowID;
                cmd.ExecuteNonQuery();
                con.Close();

                //Reply with the appropriate message.
                switch(trackingStatus) {
                    case 0:
                        return ReplyAsync("Disabled tracking for " + Context.User.Username);
                    default:
                        return ReplyAsync("Enabled tracking for " + Context.User.Username);
                }
            }
            //If the user was not found, send an error.
            return ReplyAsync("User was not found, please try again later.");
        }
    }
}
