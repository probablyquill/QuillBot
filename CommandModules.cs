using Discord.Commands;

namespace QuillBot {
    public class SayModule : ModuleBase<SocketCommandContext> {
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
            switch (GlobalVariables.TESTBOOL) {
                case true:
                    GlobalVariables.TESTBOOL = false;
                    break;
                default:
                    GlobalVariables.TESTBOOL = true;
                    break;
            }

            String output = "TESTBOOL is now " + GlobalVariables.TESTBOOL;
            return ReplyAsync(output);
        }
        
        [Command("info")]
        [Summary("Displays command list.")]
        public Task ReturnInfo() {
            String output = 
                "__Commands__:\n\n**$say** => Have the bot echo whatever was typed after 'say'.\n\n**$trackerToggle** => Toggle Online percentage status for this server.";
            output += "\n\nContact: quillbot@probablyquill.com";
            return ReplyAsync(output);
        }
    }
}
