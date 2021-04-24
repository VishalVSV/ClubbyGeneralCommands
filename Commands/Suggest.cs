using Clubby.Discord;
using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Plugins;
using Discord;
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
    public class Suggest : IDiscordCommand
    {
        public HelpDetails GetCommandHelp()
        {
            return Utils.Init((ref HelpDetails h) =>
            {
                h.CommandName = "suggest";
                h.ShortDescription = "Make a suggestion!";
            });
        }

        public DiscordCommandPermission GetMinimumPerms()
        {
            return DiscordCommandPermission.Member;
        }

        public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
        {
            string suggestion_msg = msg.Content.Substring("suggest".Length + Clubby.Program.config.DiscordBotPrefix.Length).Trim();

            if (suggestion_msg == "")
            {
                await msg.Channel.SendError("Enter a suggestion!");
                return;
            }

            ulong channel_id;
            if (Clubby.Program.config.DiscordChannels.TryGetValue("suggestions", out channel_id))
            {
                var channel = commandHandler.GetChannel(channel_id);
                if (channel != null)
                {
                    Suggestion suggestion = new Suggestion();
                    suggestion.suggestion = suggestion_msg;
                    suggestion.Author = msg.Author.Username + "#" + msg.Author.Discriminator;
                    suggestion.suggestion_number = Clubby.Program.config.DiscordSuggestionCount;
                    suggestion.score = 0;
                    suggestion.icon_url = msg.Author.GetAvatarUrl();

                    var poll = await channel.SendMessageAsync(null, false, ConstructSuggestion(suggestion));

                    suggestion.msg_id = poll.Id;

                    await poll.AddReactionAsync(new Emoji("✅"));
                    await poll.AddReactionAsync(new Emoji("❌"));

                    Clubby.Program.config.DiscordSuggestions.Add(poll.Id, suggestion);
                    Clubby.Program.config.DiscordSuggestionCount += 1;
                }
                else throw new Exception($"Suggestion channel not set!\n\nAsk an admin to set it using `{Clubby.Program.config.DiscordBotPrefix}set_channel suggestions`");
            }
            else throw new Exception($"Suggestion channel not set!\n\nAsk an admin to set it using `{Clubby.Program.config.DiscordBotPrefix}set_channel suggestions`");
        }

        public static async Task MakeRawSuggestion(SocketMessage msg, string suggestion_msg, ITextChannel channel)
        {
            Suggestion suggestion = new Suggestion();
            suggestion.suggestion = suggestion_msg;
            suggestion.Author = msg.Author.Username + "#" + msg.Author.Discriminator;
            suggestion.suggestion_number = Clubby.Program.config.DiscordSuggestionCount;
            suggestion.score = 0;
            suggestion.icon_url = msg.Author.GetAvatarUrl();

            var poll = await channel.SendMessageAsync(null, false, ConstructSuggestion(suggestion));

            suggestion.msg_id = poll.Id;

            await poll.AddReactionAsync(new Emoji("✅"));
            await poll.AddReactionAsync(new Emoji("❌"));

            Clubby.Program.config.DiscordSuggestions.Add(poll.Id, suggestion);
            Clubby.Program.config.DiscordSuggestionCount += 1;
        }

        public static Embed ConstructSuggestion(Suggestion suggestion, bool closed = false)
        {
            return new EmbedBuilder()
                .WithAuthor(suggestion.Author, suggestion.icon_url)
                .WithTitle($"Suggestion #{suggestion.suggestion_number} {(closed ? "[Closed]" : "")}")
                .WithDescription(suggestion.suggestion)
                .WithColor(GetColor(suggestion.score))
                .Build();
        }

        public static Embed ConstructAccept(Suggestion suggestion, string reason = null)
        {
            return new EmbedBuilder()
                .WithAuthor(suggestion.Author, suggestion.icon_url)
                .WithTitle($"Suggestion #{suggestion.suggestion_number} Approved!")
                .WithDescription(suggestion.suggestion +
                    (reason == null ? "" :
                    $"\n\nReason: {reason}")
                )
                .WithColor(Color.Green)
                .Build();
        }
        public static Embed ConstructDeny(Suggestion suggestion, string reason = null)
        {
            return new EmbedBuilder()
                .WithAuthor(suggestion.Author, suggestion.icon_url)
                .WithTitle($"Suggestion #{suggestion.suggestion_number} Denied!")
                .WithDescription(suggestion.suggestion +
                    (reason == null ? "" :
                    $"\n\nReason: {reason}")
                )
                .WithColor(Color.Red)
                .Build();
        }


        public static Color GetColor(int score)
        {
            if (score >= 5 && score < 10)
            {
                return new Color(255, 255, 0);
            }
            else if (score >= 10)
            {
                return Color.Blue;
            }

            return Color.DarkGrey;
        }

        public static async Task ReactionHandler(Cacheable<IUserMessage, ulong> arg1, Suggestion suggestion)
        {
            var msg = (await arg1.GetOrDownloadAsync()) as RestUserMessage;
            int upvotes = CountReactions(msg, new Emoji("✅")) - 1;
            int downvotes = CountReactions(msg, new Emoji("❌")) - 1;

            suggestion.score = upvotes - downvotes;

            Color color = Color.Red;

            color = GetColor(suggestion.score);

            await msg.ModifyAsync((m) =>
            {
                m.Embed = ConstructSuggestion(suggestion);
            });
        }

        public static int CountReactions(RestUserMessage msg, IEmote emote)
        {
            return msg.Reactions[emote].ReactionCount;
        }
    }
}
