using Discord.Commands;
using System.Data.SQLite;
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
                "__Commands__:\n\n**$say** => Have the bot echo whatever was typed after 'say'.\n\n**$trackerToggle** => Toggle Online percentage status for this server.";
            output += "\n\n$info => Displays this menu";
            output += "\n\nContact: quillbot@probablyquill.com";
            return ReplyAsync(output);
        }
        //Display User Online Time
        [Command("online")]
        [Summary("Displays the percentage of time which the user has been online.")]
        public Task OnlineInfo() {
            //Create database connection
            var con = new SQLiteConnection(Global.UserDBLocation);
            SQLiteDataReader response;
            String output = "";
            //int linesChanged;
            con.Open();

            //Connect to the database
            var cmd = new SQLiteCommand(con);

            //Select based on user's id
            cmd.CommandText = "SELECT * FROM users WHERE userid = " + Context.User.Id; 
            response = cmd.ExecuteReader();
            response.Read();

            //Output Testing
            //Console.WriteLine("ID\tUser\t\t\tOnline\tOffline\tStarted\tStatus");
            //Console.WriteLine(response.GetInt32(0) + "\t" + response.GetInt64(1) + "\t" + response.GetInt32(2) + "\t" + response.GetInt32(3) + "\t" + response.GetInt32(4) +  "\t" + response.GetInt32(5));

            //Save variables based on user's returned information.
            int rowID = response.GetInt32(0);
            int userID = response.GetInt32(1);
            double onlineCount = response.GetInt32(2);
            double offlineCount = response.GetInt32(3);
            int started = response.GetInt32(4);
            int trackingStatus = response.GetInt32(5);
            response.Close();
            con.Close();

            //Calculate online percentage
            double onlineTime = 100;
            if (offlineCount != 0) {
                onlineTime = onlineCount / (offlineCount + onlineCount);
                onlineTime = onlineTime * 100;
            }

            double hourtotal = 15 * onlineCount;
            hourtotal = hourtotal / 60;
            hourtotal = hourtotal * 100;
            int hourInt = (int) hourtotal;
            hourtotal = hourInt / 100;
            //Message String
            output += "You have been online " + (int) onlineTime + "% of the time since tracking started, or approximately " + hourtotal + " minutes.";
            return ReplyAsync(output);
        }
    }
}
