using Clubby.Discord;
using Clubby.EventManagement;
using Clubby.Plugins;
using Clubby.Scheduling;
using ClubbyGeneralCommands.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ClubbyGeneralCommands
{
    [PluginExport, PluginInit]
    public static class StaticData
    {
        private static ulong ignore_id;

        private static readonly Game[] statuses = new Game[]
        {
            new Game("Pumped up kicks",ActivityType.Listening),
            new Game("WSDC 2015 Finals",ActivityType.Listening,ActivityProperties.Instance,"Singapore vs Pakistan"),
            new Game("How to win debates for losers",ActivityType.Listening),
            new Game("Strats to beat CV",ActivityType.Listening),
            new Game("Debate castle simulator",ActivityType.Playing)
        };
        private static int current_status = 0;

        private static bool christmas_done = false;
        private static bool friday_done = false;

        public static void Init(DiscordBot data)
        {
            data.MessageReceivedHandler.On("MessageReceived", SuggestionHandler, 4);

            ignore_id = data.client.CurrentUser.Id;

            PeriodicEvent status_changer = new PeriodicEvent
            {
                interval = TimeSpan.FromMinutes(30)
            };
            status_changer.last_exec = DateTime.Now - status_changer.interval;
            status_changer.action = () =>
            {
                DateTime now = DateTime.Now;
                if (now.Day == 13 && now.DayOfWeek == DayOfWeek.Friday)
                {
                    if (!friday_done)
                    {
                        data.client.SetActivityAsync(new Game("Spooky scary skeletons", ActivityType.Listening));
                        friday_done = true;
                    }
                }
                else if (now.Day == 25 && now.Month == 12)
                {
                    if (!christmas_done)
                    {
                        var guild = data.client.GetGuild(Clubby.Program.config.DiscordGuildId);
                        if (guild != null)
                        {
                            var channels = guild.TextChannels;
                            foreach (var channel in channels)
                            {
                                if (channel.Name == "slack")
                                {
                                    channel.SendMessageAsync("Merry christmas!");
                                }
                            }
                        }

                        data.client.SetActivityAsync(new Game("Christmas carols", ActivityType.Listening));
                        christmas_done = true;
                    }
                }
                else
                {
                    if (christmas_done)
                        christmas_done = false;
                    if (friday_done)
                        friday_done = false;

                    current_status %= statuses.Length;
                    data.client.SetActivityAsync(statuses[current_status]);

                    current_status++;
                }
            };

            Clubby.Program.config.scheduler.PeriodicActivities.Add(status_changer);

            PeriodicEvent DashboardUpdate = new PeriodicEvent
            {
                interval = TimeSpan.FromMinutes(5)
            };
            DashboardUpdate.last_exec = DateTime.Now - DashboardUpdate.interval;
            DashboardUpdate.action = async () =>
            {
                await Clubby.Program.config.DiscordBot.UpdateDashboard();
            };

            Clubby.Program.config.scheduler.PeriodicActivities.Add(DashboardUpdate);

            data.client.ReactionAdded += Client_ReactionAdded;
            data.client.ReactionRemoved += Client_ReactionRemoved;
        }

        private static EventResult SuggestionHandler(SocketMessage msg)
        {
            if (Clubby.Program.config.DiscordChannels.ContainsKey("suggestions") && msg.Channel.Id == Clubby.Program.config.DiscordChannels["suggestions"])
            {
                msg.DeleteAsync().Wait();
                Suggest.MakeRawSuggestion(msg, msg.Content, msg.Channel as SocketTextChannel).Wait();

                return EventResult.Stop;
            }
            return EventResult.Continue;
        }

        private static async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.UserId == ignore_id)
                return;

            var msg = (await arg1.GetOrDownloadAsync()).Id;
            if (Clubby.Program.config.DiscordSuggestions.ContainsKey(msg))
            {
                _ = Suggest.ReactionHandler(arg1, Clubby.Program.config.DiscordSuggestions[msg]);
            }
        }

        private static async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.UserId == ignore_id)
                return;

            var msg = (await arg1.GetOrDownloadAsync()).Id;
            if (Clubby.Program.config.DiscordSuggestions.ContainsKey(msg))
            {
                _ = Suggest.ReactionHandler(arg1, Clubby.Program.config.DiscordSuggestions[msg]);
            }
        }
    }
}
