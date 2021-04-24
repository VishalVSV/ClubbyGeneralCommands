using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace ClubbyGeneralCommands.Commands
{
    [PluginExport]
    public class set_channel : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "set_channel";
                h.ShortDescription = "Set current channel to something special";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            if (msg.Content.Length > "set_channel".Length + Clubby.Program.config.DiscordBotPrefix.Length + 1)
            {
                string channel_name = msg.Content.Substring("set_channel".Length + Clubby.Program.config.DiscordBotPrefix.Length + 1).Trim();

                if (channel_name != "")
                {
                    if (Clubby.Program.config.DiscordChannels.ContainsKey(channel_name))
                        Clubby.Program.config.DiscordChannels[channel_name] = msg.Channel.Id;
                    else
                        Clubby.Program.config.DiscordChannels.Add(channel_name, msg.Channel.Id);
                }
                else throw new Exception("Expected name for the channel");
            }
            else throw new Exception("Expected name for the channel");
        }
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously