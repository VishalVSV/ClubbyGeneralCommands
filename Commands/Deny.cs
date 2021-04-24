using Clubby.Discord;
using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubbyGeneralCommands.Commands
{
    [PluginExport]
    public class Deny : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "deny";
                h.ShortDescription = "Deny a suggestion!";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        private bool success = false;
        private string reason;

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            if (msg.Content.Split(' ').Length < 3)
                throw new Exception("Expected two parameters, the suggestion id and a reason!");

            reason = msg.Content.Substring(msg.Content.IndexOf(' ') + 1 + msg.Content.Split(' ')[1].Length).Trim();

            DenySuggestion(int.Parse(msg.Content.Split(' ')[1]));
            if (success)
                await msg.Channel.SendOk("Suggestion Denied!");
        }

        private void DenySuggestion(int index)
        {
            var keys = Clubby.Program.config.DiscordSuggestions.Keys.ToList();
            ulong key = 0;
            bool got_key = false;

            for (int i = 0; i < keys.Count; i++)
            {
                if (Clubby.Program.config.DiscordSuggestions[keys[i]].suggestion_number == index)
                {
                    got_key = true;
                    key = keys[i];
                    break;
                }
            }
            if (!got_key)
                throw new Exception($"Couldn't find suggestion #{index}");

            Suggestion suggestion = Clubby.Program.config.DiscordSuggestions[key];
            Clubby.Program.config.DiscordSuggestions.Remove(key);

            success = true;
            ulong channel_id;
            if (Clubby.Program.config.DiscordChannels.TryGetValue("suggestions", out channel_id))
            {
                var channel = Clubby.Program.config.GetChannel(channel_id);
                if (channel != null)
                {
                    var msg = channel.GetMessageAsync(suggestion.msg_id).Result as RestUserMessage;

                    msg.ModifyAsync((m) =>
                    {
                        m.Embed = Suggest.ConstructSuggestion(suggestion, true);
                    });

                    channel.SendMessageAsync(null, false, Suggest.ConstructDeny(suggestion, reason));
                }
            }

            ulong council_channel_id;
            if (Clubby.Program.config.DiscordChannels.TryGetValue("council_suggestions", out council_channel_id))
            {
                var channel = Clubby.Program.config.GetChannel(council_channel_id);
                if (channel != null)
                {
                    channel.SendMessageAsync(null, false, Suggest.ConstructDeny(suggestion, reason));
                }
            }
        }
    }
}
