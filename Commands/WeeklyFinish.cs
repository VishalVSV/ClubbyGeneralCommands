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
    public class Weekly_Finish : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "weekly_finish";
                h.ShortDescription = "Finish a weekly debate";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        public int Stage = 0;
        private List<IMessage> messages = new List<IMessage>();
        private string Judges = null, Remarks = null;
        private int index;
        private Debate debate;

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            var exec = commandHandler.GetExecutingCommand(msg.Author.Id);
            if (exec == null)
            {
                try
                {
                    Dispatcher.Dispatch<int>(HandleFinish, msg.Content.Substring(Clubby.Program.config.DiscordBotPrefix.Length + "weekly_finish".Length).Trim(), "Expected debate number to finish!");
                    commandHandler.SetExecutingCommand(msg.Author.Id, this);
                    Stage++;
                    messages.Add(await msg.Channel.SendMessageAsync("Enter judges."));
                }
                catch (Exception e)
                {
                    messages.Add(await msg.Channel.SendError(e.Message));
                }
            }
            else if (exec == this)
            {
                if (Stage == 1)
                {
                    messages.Add(msg);
                    Judges = msg.Content.Trim();
                    Stage++;
                    messages.Add(await msg.Channel.SendMessageAsync("Enter Remarks."));
                }
                else if (Stage == 2)
                {
                    messages.Add(msg);
                    Remarks = msg.Content.Trim();
                    await msg.Channel.SendOk($"Finished debate #{index}");

                    commandHandler.SetExecutingCommand(msg.Author.Id, null);

                    try
                    {
                        if (Clubby.Program.config.scheduleSheetsHandler != null)
                        {
                            debate.Judges = Judges;
                            debate.Remarks = Remarks;

                            Clubby.Program.config.UnfinishedDebates.Remove(index);

                            await (msg.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

                            for (int i = 0; i < 5; i++)
                            {

                                try
                                {
                                    await Clubby.Program.config.scheduleSheetsHandler.AddEvent(debate);
                                    break;
                                }
                                catch (Exception)
                                {
                                    await Task.Delay(500);
                                    continue;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        await msg.Channel.SendError(e.Message);
                    }
                }
            }


        }

        public void HandleFinish(int index)
        {
            if (Clubby.Program.config.UnfinishedDebates.ContainsKey(index))
            {
                this.index = index;
                debate = Clubby.Program.config.UnfinishedDebates[index];
            }
            else throw new Exception($"Debate {index} doesn't exist");
        }
    }
}
