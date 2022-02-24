using Discord.Commands;
using System.Data.SQLite;
using Discord.WebSocket;
using Discord;

namespace QuillBot {
    public class MainModule : ModuleBase<SocketCommandContext> {
        //Say Hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] String echo) => ReplyAsync(echo);
        
        //Info Command
        [Command("info")]
        [Summary("Displays command list.")]
        public Task ReturnInfo() {
            String output = 
                "__Commands__:\n**$say** => Have the bot echo whatever was typed after 'say'.\n\n**$servertoggle** => Toggle online percentage tracking for this server (Not implemented).";
            output += "\n\n**$toggle** => Toggle whether the bot will track your online status. Default is on. This is not server specific.";
            output += "\n\n**$online** => Displays the percentage of time a user has been online. Can also be called at a user, such as $online @QuillBot";
            output += "\n\n**$drop** -> Deletes your information from the database. This includes whether you have tracking toggled on and off, and is meant for resetting stats.";
            output += "\n\n**$info** => Displays this menu";
            output += "\n\n*Contact: quill@probablyquill.com*";
            return ReplyAsync(output);
        }
        //Display User Online Time
        [Command("online")]
        [Summary("Displays the percentage of time which the user has been online.")]
        public Task OnlineInfo(SocketUser user = null) {
            //Create database connection
            var con = new SQLiteConnection(Global.DBLocation);
            SQLiteDataReader response;
            String output = "";
            //int linesChanged;
            con.Open();

            //Connect to the database
            var cmd = new SQLiteCommand(con);

            //Check if tracking is disabled in the server.
            cmd.CommandText = "SELECT * FROM togglelist WHERE serverid = " + Context.Guild.Id;
            response = cmd.ExecuteReader();
            
            if (response.HasRows) {
                response.Read();
                int allowed = response.GetInt32(2);
                response.Close();
                if (allowed == 0) return ReplyAsync("Sorry, tracking is disabled in this server.");
            }
            
            response.Close();
            
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

                    String measurement = "hours";
                    int hourInt;
                    double hourtotal = 15 * onlineCount;
                    hourtotal = hourtotal / (60 * 60);

                    //If less than one hour, read out in minutes instead.
                    if (hourtotal < 1) {
                        hourtotal = hourtotal * 60;
                        measurement = "minutes";
                    }

                    //Round to hundreds decimal place.
                    hourtotal = hourtotal * 100;
                    hourInt = (int) hourtotal;
                    hourtotal = (double) hourInt / 100;
                    
                    //Message String
                    if (user == null) {
                        output += "You have been online " + (int) onlineTime + "% of the time since tracking started, approximately " + hourtotal + " " + measurement + ".";
                        if (hourtotal == 0) {
                            output += "\nYet you still called this command. Curious.";
                        }
                        
                    } else {
                        output += "" + user.Username + " has been online " + (int) onlineTime + "% of the time since tracking started, approximately " + hourtotal  + " " + measurement + ".";
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
            var con = new SQLiteConnection(Global.DBLocation);
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
        [Command("drop")]
        [Summary("Removes the specified user's data.")]
        public Task DropUser(SocketUser user = null) {
            //Create database connection
            var con = new SQLiteConnection(Global.DBLocation);
            //SQLiteDataReader response;
            //int linesChanged;
            con.Open();

            //Connect to the database
            var cmd = new SQLiteCommand(con);
            String output = "";

            //First check: Is the user calling it without an argument, or with the argument being themself?
            if (user == null || user.Id == Context.User.Id) {
                cmd.CommandText = "DELETE FROM users WHERE userid = " + Context.User.Id;
                int reply = cmd.ExecuteNonQuery();
                switch (reply) {
                    case 0:
                        output += "Error: Nothing was found to delete.";
                    break;
                    case 1:
                        output += "Deleted table for " + Context.User.Username;
                        break;
                    default:
                        output +="Something went wrong and more than one row was deleted, I hope there's a backup.";
                        break;
                }
            //If not, is it me calling it?
            } else if ((Context.User.Id == 272123456995196928) && (user != null)){
                cmd.CommandText = "DELETE FROM users WHERE userid = " + user.Id;
                int reply = cmd.ExecuteNonQuery();
                switch (reply) {
                    case 0:
                        output += "Error: Nothing was found to delete.";
                    break;
                    case 1:
                        output += "Deleted table for " + user.Username;
                        break;
                    default:
                        output +="Something went wrong and more than one row was deleted, I hope there's a backup.";
                        break;
                }
            //If it doesn't fit the above, then it is not allowed.
            } else {
                output += "Sorry, only Quill is allowed to do that.";
            }
            return ReplyAsync(output);
        }
        [RequireOwner]
        [Command("servertoggle")]
        [Summary("Toggle tracking inside of the current server.")]
        public Task ToggleServerTracking() {

            //Create database connection
            var con = new SQLiteConnection(Global.DBLocation);
            SQLiteDataReader response;
            //int linesChanged;
            con.Open();

            //Connect to the database
            var cmd = new SQLiteCommand(con);
            String output = "";

            cmd.CommandText = "SELECT * FROM togglelist WHERE serverid = " + Context.Guild.Id;
            response = cmd.ExecuteReader();

            //Store tracking status for output
            int tracking = 1;
            if (response.HasRows) {
                response.Read();
                int rowID = response.GetInt32(0);
                tracking = response.GetInt32(2);
                response.Close();

                switch(tracking) {
                    case 0:
                        output += "Tracking for this server has been enabled.";
                        tracking = 1;
                        break;
                    default:
                        output += "Tracking for this server has been disabled.";
                        tracking = 0;
                        break;
                }

                cmd.CommandText = "UPDATE togglelist SET toggled = " + tracking + " WHERE serverid = " + Context.Guild.Id;
                cmd.ExecuteNonQuery();
            }
            return ReplyAsync(output);
            //TODO: Add user permission checking - require administrator to change.
        }
    }
}
