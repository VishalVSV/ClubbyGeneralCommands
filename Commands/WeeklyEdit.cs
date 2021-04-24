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
    public class Weekly_Edit : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "weekly_edit";
                h.ShortDescription = "Edit an announcement";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        private int Progress = -1;
        private List<IMessage> messages = new List<IMessage>();

        private int index;
        private ulong edited_message;

        private DateTime date_time;
        private string Prop, Opp, Motion;

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            var exec = commandHandler.GetExecutingCommand(msg.Author.Id);

            if (exec == this)
            {
                if (Progress == 0)
                {
                    DateTime date_time;
                    if (DateTime.TryParseExact(msg.Content, new string[] { "d/M/yyyy H:m", "d/M/yyyy h:m tt", "d/M/yyyy h tt", "d/M/yyyy H" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date_time))
                    {
                        this.date_time = date_time;
                        messages.Add(msg);
                        messages.Add(await msg.Channel.SendMessageAsync("Enter proposition team name/number"));
                        Progress += 1;
                    }
                    else
                    {
                        messages.Add(await msg.Channel.SendMessageAsync("Please enter a valid date and time in dd/mm/yyyy hh:mm PM/AM form"));
                    }
                }
                else if (Progress == 1)
                {
                    Prop = msg.Content.Trim();
                    Progress += 1;
                    messages.Add(msg);
                    messages.Add(await msg.Channel.SendMessageAsync("Enter opposition team name/number"));
                }
                else if (Progress == 2)
                {
                    Opp = msg.Content.Trim();
                    Progress += 1;
                    messages.Add(msg);
                    messages.Add(await msg.Channel.SendMessageAsync("Enter motion"));
                }
                else if (Progress == 3)
                {
                    Motion = msg.Content.Trim();
                    Progress += 1;
                    messages.Add(msg);

                    var channel = guild.GetChannel(Clubby.Program.config.DiscordChannels["announcements"]) as SocketTextChannel;

                    await ((await channel.GetMessageAsync(edited_message)) as RestUserMessage).ModifyAsync((m) =>
                    {
                        m.Embed = new EmbedBuilder()
                            .WithTitle($"Announcement #{index}")
                            .WithAuthor(msg.Author)
                            .WithColor(Color.Blue)
                            .WithDescription($"The next debate will take place on {date_time.DayOfWeek.ToString()}({date_time.Day}/{date_time.Month}/{date_time.Year}) at {new DateTime(date_time.TimeOfDay.Ticks).ToString("h:mm tt")}")
                            .AddField("Proposition:", Prop)
                            .AddField("Opposition:", Opp)
                            .AddField($"The motion is:", Motion)
                            .Build();
                    });
                    //new EmbedBuilder()
                    //    .WithTitle($"Announcement #{Clubby.Program.config.DiscordAnnouncements.Count}")
                    //    .WithAuthor(msg.Author)
                    //    .WithColor(Color.Blue)
                    //    .WithDescription($"The next debate will take place on {date_time.DayOfWeek.ToString()}({date_time.Day}/{date_time.Month}/{date_time.Year}) at {new DateTime(date_time.TimeOfDay.Ticks).ToString("h: mm tt")}")
                    //    .AddField("Proposition:", Prop)
                    //    .AddField("Opposition:", Opp)
                    //    .AddField($"The motion is:", Motion)
                    //    .Build()

                    Clubby.Program.config.scheduler.ReSchedule("Weeklies", index, date_time - TimeSpan.FromMinutes(5));

                    await msg.Channel.SendOk("Announcement Edited!");

                    await (msg.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

                    commandHandler.SetExecutingCommand(msg.Author.Id, null);
                }
            }
            else if (exec == null)
            {
                if (msg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 2)
                {
                    if (ulong.TryParse(msg.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], out edited_message))
                    {
                        if (edited_message < (ulong)Clubby.Program.config.DiscordAnnouncements.Count)
                        {
                            index = (int)edited_message;
                            if (Clubby.Program.config.DiscordAnnouncements[index].Item1 == Clubby.Discord.AnnouncementType.Weekly)
                            {
                                edited_message = Clubby.Program.config.DiscordAnnouncements[index].Item2;
                                messages.Add(await msg.Channel.SendMessageAsync("Enter a valid date and time in dd/mm/yyyy hh:mm PM/AM form"));
                                Progress += 1;
                                commandHandler.SetExecutingCommand(msg.Author.Id, this);
                            }
                            else
                                await msg.Channel.SendError($"Can't edit normal announcements using weekly_edit!");
                        }
                        else
                            await msg.Channel.SendError($"Couldn't find announcement #{edited_message}");
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
