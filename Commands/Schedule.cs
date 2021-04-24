using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ClubbyGeneralCommands.Commands
{
    [PluginExport]
    public class Schedule : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "schedule";
                h.ShortDescription = "Check what's upcoming";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Member;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            await msg.Channel.SendMessageAsync(null, false, ConstructSchedule().WithAuthor(msg.Author).Build());
        }

        private EmbedBuilder ConstructSchedule()
        {
            var res = new EmbedBuilder()
                .WithTitle("Schedule");
            bool events_found = false;

            if (Clubby.Program.config.scheduler.Scheduler_raw.Count > 0)
            {
                foreach (var events in Clubby.Program.config.scheduler.Scheduler_raw.Values)
                {
                    for (int i = 0; i < Math.Min(25, events.Count); i++)
                    {
                        string formatting = events.Keys[i].Date == DateTime.Today ? "__" : "";
                        events_found = true;
                        res.AddField($"{formatting}{i + 1}. " + events.Values[i].Item2.name + formatting, "**@** " + events.Keys[i].ToString("dddd dd/MM/yyyy hh:mm tt"));
                    }
                }
            }
            if(!events_found)
            {
                res.WithDescription("No debates upcoming!");
            }

            return res;
        }
    }
}
