using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClubbyGeneralCommands.Commands
{
    [PluginExport]
    public class Unfinished_Debates : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "unfinished_debates";
                h.ShortDescription = "List the debates that haven't been finished";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            if(Clubby.Program.config.UnfinishedDebates.Count == 0)
            {
                await msg.Channel.SendOk("There are no unfinished debates!");
                return;
            }

            StringBuilder debates = new StringBuilder();
            int i = 1;
            foreach (var (debate_num,debate) in Clubby.Program.config.UnfinishedDebates)
            {
                debates.AppendLine($"{i}. Debate #{debate_num} b/w {debate.Proposition} and {debate.Opposition}");
            }

            await msg.Channel.SendMessageAsync(null, false,
                new EmbedBuilder()
                .WithTitle("Unfinised Debates")
                .WithColor(Color.Blue)
                .WithDescription("I don't know the judges and remarks for these debates. So I haven't put it on the register yet.  ¯\\_(ツ)_/¯")
                .AddField("Debates:", debates.ToString())
                .Build());
        }
    }
}
