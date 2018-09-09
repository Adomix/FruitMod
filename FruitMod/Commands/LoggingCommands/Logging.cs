using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Services;
using System.Linq;
using System.Threading.Tasks;

namespace FruitMod.Commands.LoggingCommands
{
    public class Logging : ModuleBase<SocketCommandContext>
    {
        private GuildService _guildService;

        public Logging(GuildService guildService)
        {
            _guildService = guildService;
        }

        [Command("snipe")]
        [Summary("snipes the last deleted mention")]
        public async Task Snipe()
        {
            var message = _guildService.delmsgs[Context.Guild.Id].LastOrDefault(x => x.MentionedUsers.Count > 0);
            if (message != null)
            {
                var author = message.Author as SocketGuildUser;
                var color = (message.Author as SocketGuildUser).Roles.LastOrDefault(x => x.Color != Color.Default).Color;
                if (color == null)
                    color = Color.DarkPurple;
                var embed = new EmbedBuilder()
                    .WithCurrentTimestamp()
                    .WithColor(color)
                    .WithAuthor(author)
                    .WithTitle($"Sniped!")
                    .AddField($"Mentions: ({message.MentionedUsers.Count})", string.Join(", ", message.MentionedUsers))
                    .AddField($"Content:", message.Content)
                    .Build();
                await ReplyAsync(string.Empty, false, embed);
            }
            else
                await ReplyAsync("No messages with mentions found!");
        }

        [Command("grab")]
        [Summary("grabs the last deleted message")]
        public async Task Grab()
        {
            var message = _guildService.delmsgs[Context.Guild.Id].LastOrDefault(x => x.MentionedUsers.Count == 0);
            if (message != null)
            {
                var author = message.Author as SocketGuildUser;
                var color = (message.Author as SocketGuildUser).Roles.LastOrDefault(x => x.Color != Color.Default).Color;
                if (color == null)
                    color = Color.DarkPurple;
                var embed = new EmbedBuilder()
                    .WithCurrentTimestamp()
                    .WithColor(color)
                    .WithAuthor(author)
                    .WithTitle($"Grabbed the last deleted message!")
                    .AddField($"Content:", message.Content)
                    .Build();
                await ReplyAsync(string.Empty, false, embed);
            }
            else
                await ReplyAsync("No messages found!");
        }
    }
}