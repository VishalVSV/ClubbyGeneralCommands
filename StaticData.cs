using Clubby.Discord;
using Clubby.EventManagement;
using Clubby.Plugins;
using Clubby.Scheduling;
using ClubbyGeneralCommands.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Text;
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

        private static bool Y2hyaXN0bWFzX2RvbmU = false;
        private static bool ZnJpZGF5X2RvbmU = false;

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
                    if (!ZnJpZGF5X2RvbmU)
                    {
                        data.client.SetActivityAsync(new Game(Encoding.UTF8.GetString(Convert.FromBase64String("U3Bvb2t5IHNjYXJ5IHNrZWxldG9ucw==")), ActivityType.Listening));
                        ZnJpZGF5X2RvbmU = true;
                    }
                }
                else if (now.Day == 25 && now.Month == 12)
                {
                    if (!Y2hyaXN0bWFzX2RvbmU)
                    {
                        var guild = data.client.GetGuild(Clubby.Program.config.DiscordGuildId);
                        if (guild != null)
                        {
                            var channels = guild.TextChannels;
                            foreach (var channel in channels)
                            {
                                if (channel.Name == "slack")
                                {
                                    channel.SendMessageAsync(Encoding.UTF8.GetString(Convert.FromBase64String("TWVycnkgY2hyaXN0bWFzIQ==")));
                                }
                            }
                        }

                        data.client.SetActivityAsync(new Game(Encoding.UTF8.GetString(Convert.FromBase64String("Q2hyaXN0bWFzIGNhcm9scw==")), ActivityType.Listening));
                        Y2hyaXN0bWFzX2RvbmU = true;
                    }
                }
                else
                {
                    if (Y2hyaXN0bWFzX2RvbmU)
                        Y2hyaXN0bWFzX2RvbmU = false;
                    if (ZnJpZGF5X2RvbmU)
                        ZnJpZGF5X2RvbmU = false;

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
