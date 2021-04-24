using Clubby.Discord;
using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClubbyGeneralCommands.Commands
{
    [PluginExport]
    public class Make_Dashboard : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "make_dashboard";
                h.ShortDescription = "Make the bot dashboard";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            DiscordMessage dash = new DiscordMessage();
            dash.MessageId = (await msg.Channel.SendMessageAsync(null, false, Clubby.Program.config.DiscordBot.GetDashboard(!Clubby.Program.stop))).Id;
            dash.ChannelId = msg.Channel.Id;

            Clubby.Program.config.DiscordDashboard = dash;
        }
    }
}
