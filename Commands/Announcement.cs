using Clubby.Discord;
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
    public class Announcement : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "announcement";
                h.ShortDescription = "Make a general announcement";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            var exec = commandHandler.GetExecutingCommand(msg.Author.Id);
            if (exec == this)
            {
                SocketTextChannel channel = guild.GetChannel(Clubby.Program.config.DiscordChannels["announcements"]) as SocketTextChannel;

                if (channel != null)
                {
                    int id = Clubby.Program.config.DiscordAnnouncements.Count;
                    var ann = await channel.SendMessageAsync(null, false, new EmbedBuilder()
                        .WithTitle($"Announcement #{id}")
                        .WithAuthor(msg.Author)
                        .WithColor(Color.Blue)
                        .WithDescription(msg.Content)
                        .Build());

                    Clubby.Program.config.DiscordAnnouncements.Add((AnnouncementType.Normal, ann.Id));

                    await msg.Channel.SendOk("Announcement sent!");
                }
                else throw new Exception("Annoucement channel is not set!");

                commandHandler.SetExecutingCommand(msg.Author.Id, null);
            }
            else if(exec == null)
            {
                await msg.Channel.SendOk("Enter the announcement to send");
                commandHandler.SetExecutingCommand(msg.Author.Id, this);
            }
        }
    }
}
