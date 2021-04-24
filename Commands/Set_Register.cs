using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClubbyGeneralCommands.Commands
{
    [PluginExport]
    public class Set_Register : IDiscordCommand
    {
        private static Regex regex;

        static Set_Register()
        {
            regex = new Regex(@"https:\/\/docs\.google\.com\/spreadsheets\/d\/(?<id>.*)\/edit\?*");
        }

        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "set_register";
                h.ShortDescription = "Set sheet file";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Admin;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            if (msg.Content.Split(' ').Length < 3)
                throw new Exception("Expected 2 arguments!");

            string url = msg.Content.Substring(Clubby.Program.config.DiscordBotPrefix.Length + "set_register".Length).Trim().Substring(0, msg.Content.Substring(Clubby.Program.config.DiscordBotPrefix.Length + "set_register".Length).Trim().IndexOf(' ') + 1);
            string sheetName = msg.Content.Substring(Clubby.Program.config.DiscordBotPrefix.Length + "set_register".Length).Trim().Substring(msg.Content.Substring(Clubby.Program.config.DiscordBotPrefix.Length + "set_register".Length).Trim().IndexOf(' ')).Trim();

            HandleMsg(url, sheetName);
            await msg.Channel.SendOk("Register set!");
        }

        public void HandleMsg(string url,string sheetName)
        {
            Match match;
            if((match = regex.Match(url)) != null)
            {
                Logger.Log(this, match.Groups["id"].Value);
                Clubby.Program.config.RegisterScheduleSheetName = sheetName;
                Clubby.Program.config.RegisterFileId = match.Groups["id"].Value;
            }
            else
            {
                throw new Exception("Url is not of correct format!");
            }
        }
    }
}
