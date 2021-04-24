using Clubby.Club;
using Clubby.Discord;
using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Clubby.Scheduling;
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
    public class Weekly_Make : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "weekly_make";
                h.ShortDescription = "Make weekly announcements";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Moderator;
        }

        private int Progress = -1;
        private List<IMessage> messages = new List<IMessage>();

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

                    if (channel != null)
                    {
                        var announcement = (await channel.SendMessageAsync(null, false,
                            new EmbedBuilder()
                            .WithTitle($"Announcement #{Clubby.Program.config.DiscordAnnouncements.Count}")
                            .WithAuthor(msg.Author)
                            .WithColor(Color.Blue)
                            .WithDescription($"The next debate will take place on {date_time.DayOfWeek.ToString()}({date_time.Day}/{date_time.Month}/{date_time.Year}) at {new DateTime(date_time.TimeOfDay.Ticks).ToString("h:mm tt")}")
                            .AddField("Proposition:", Prop)
                            .AddField("Opposition:", Opp)
                            .AddField($"The motion is:", Motion)
                            .Build())).Id;

                        SchedulerEvent reminder = new SchedulerEvent();
                        reminder.channel = channel.Id;
                        reminder.message = null;
                        reminder.embed = new EmbedBuilder().WithColor(Color.Green).WithTitle("Reminder").WithDescription($"Team {Prop} and Team {Opp} have a debate in 5 mins").AddField("Motion:", Motion);
                        reminder.name = "Weekly Debate";

                        Debate debate = new Debate();
                        debate.Context = "-";
                        debate.Motion = Motion;
                        debate.Proposition = Prop;
                        debate.Opposition = Opp;
                        debate.date = date_time;
                        try
                        {
                            Clubby.Program.config.UnfinishedDebates.Add(Clubby.Program.config.DiscordAnnouncements.Count, debate);
                            Clubby.Program.config.scheduler.Schedule("Weeklies", Clubby.Program.config.DiscordAnnouncements.Count, reminder, date_time - TimeSpan.FromMinutes(5));

                            Clubby.Program.config.DiscordAnnouncements.Add((AnnouncementType.Weekly, announcement));
                        }
                        catch(Exception e)
                        {
                            await msg.Channel.SendError(e.Message);
                        }

                        await msg.Channel.SendOk("Announcement Made!");
                    }
                    else
                    {
                        await msg.Channel.SendError("Announcement channel not set!");
                    }

                    await (msg.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

                    commandHandler.SetExecutingCommand(msg.Author.Id, null);
                }
            }
            else if (exec == null)
            {
                messages.Add(await msg.Channel.SendMessageAsync("Enter a valid date and time in dd/mm/yyyy hh:mm PM/AM form"));
                Progress += 1;
                commandHandler.SetExecutingCommand(msg.Author.Id, this);
            }
        }
    }
}
