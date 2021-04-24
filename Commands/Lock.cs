using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClubbyGeneralCommands.Commands
{
    [PluginExport]
    public class Lock : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "lock";
                h.ShortDescription = "Locks the current voice channel";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Member;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            SocketRole everyone = guild.EveryoneRole;

            var user = msg.Author as SocketGuildUser;
            if (user != null)
            {
                if (user.VoiceChannel != null)
                {
                    if (user.VoiceChannel.GetPermissionOverwrite(everyone).GetValueOrDefault().Connect == Discord.PermValue.Deny)
                    {
                        await user.VoiceChannel.AddPermissionOverwriteAsync(everyone, Discord.OverwritePermissions.InheritAll.Modify(connect: Discord.PermValue.Allow));
                        await msg.Channel.SendOk($"Channel <#{user.VoiceChannel.Id}> successfully unlocked!");
                    }
                    else
                    {
                        await user.VoiceChannel.AddPermissionOverwriteAsync(everyone, Discord.OverwritePermissions.InheritAll.Modify(connect: Discord.PermValue.Deny));
                        await msg.Channel.SendOk($"Channel <#{user.VoiceChannel.Id}> successfully locked!");
                    }
                }
                else
                {
                    await msg.Channel.SendError($"You aren't currently in a voice channel!");
                }
            }
        }
    }
}
