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
            var message = _guildService.delmsgs.Select(x => x.Value).LastOrDefault(x => x.MentionedUsers.Count > 0);
            if (message != null)
            {
                await ReplyAsync($"Sniped!\n{Format.Code($"Author:\n--------\n{message.Author}\n\nMentions:\n--------\n{string.Join(", ",message.MentionedUsers)}\n\nContent:\n--------\n{message.Content}","md")}");
            }
            else
                await ReplyAsync("No messages with mentions found!");
        }

        [Command("grab")]
        [Summary("grabs the last deleted message")]
        public async Task Grab()
        {
            var message = _guildService.delmsgs.LastOrDefault().Value;
            if (message != null)
            {
                await ReplyAsync($"Grabbed!\n{Format.Code($"Author:\n--------\n{message.Author}\n\nContent:\n--------\n{message.Content}","md")}");
            }
            else
                await ReplyAsync("No messages found!");
        }
    }
}