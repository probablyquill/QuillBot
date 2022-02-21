using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace QuillBot {
    public class CommandHandler {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        
        public CommandHandler(DiscordSocketClient client, CommandService commands) {
            //Constructor
            _client = client;
            _commands = commands;
        }

        public async Task InstallCommandsAsync() {
            //I don't really know what this does, I am just following the documentation.
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParameters) {
            //Ensure that the message sent was sent by a User and not the System.
            var message = messageParameters as SocketUserMessage;
            if (message == null) return;

            //Track prefix length/position
            int argPos = 0;

            //Check if the message is a command and prevent bots from triggering commands.
            if(!(message.HasCharPrefix('$', ref argPos)) || (message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || (message.Author.IsBot)) return;

            //Create a command context based on the message- read documentation on this later.
            var context = new SocketCommandContext(_client, message);

            //Execute the command using the created context.
            await _commands.ExecuteAsync(context: context, argPos: argPos, services: null);
        }
    }
}