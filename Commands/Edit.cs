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
    public class Edit : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "edit";
                h.ShortDescription = "Edit a general announcement";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        int index;
        ulong msg;

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            var exec = commandHandler.GetExecutingCommand(msg.Author.Id);
            if (exec == this)
            {
                var channel = guild.GetChannel(Clubby.Program.config.DiscordChannels["announcements"]) as SocketTextChannel;

                if (channel != null)
                {
                    await (await channel.GetMessageAsync(this.msg) as RestUserMessage).ModifyAsync((m) =>
                    {
                        m.Embed = new EmbedBuilder()
                        .WithTitle($"Announcement #{index}")
                        .WithAuthor(msg.Author)
                        .WithColor(Color.Blue)
                        .WithDescription(msg.Content)
                        .Build();
                    });
                    await msg.Channel.SendOk($"Announcement #{index} edited successfully!");
                }
                else
                    await msg.Channel.SendError("Announcement channel is not set!");

                commandHandler.SetExecutingCommand(msg.Author.Id, null);
            }
            else if (exec == null)
            {
                if (msg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 2)
                {
                    if (int.TryParse(msg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], out index))
                    {
                        if (index < Clubby.Program.config.DiscordAnnouncements.Count)
                        {
                            if (Clubby.Program.config.DiscordAnnouncements[index].Item1 == AnnouncementType.Normal)
                            {
                                this.msg = Clubby.Program.config.DiscordAnnouncements[index].Item2;
                                await msg.Channel.SendOk($"Enter edited announcement!");
                                commandHandler.SetExecutingCommand(msg.Author.Id, this);
                            }
                            else
                                await msg.Channel.SendError($"Can't edit weekly announcements using edit!");
                        }
                        else
                            await msg.Channel.SendError($"Couldn't find announcement #{index}");
                    }
                    else
                        await msg.Channel.SendError("Expected announcement id!\nEnter a number");
                }
                else
                {
                    await msg.Channel.SendError("Expected 2 parameters!");
                }
            }
        }
    }
}
