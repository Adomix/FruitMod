using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Preconditions;
using System.Threading.Tasks;

namespace FruitMod.Commands.FeedbackCommands
{
    [RequireContext(ContextType.DM)]
    public class OwnerReplyCommand : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public OwnerReplyCommand(DiscordSocketClient client)
        {
            _client = client;
        }
        [RequireGuildOwner]
        [Command("reply")]
        [Summary("replys to the bot's join message to the owner")]
        public async Task OwnerReply([Remainder] string message)
        {
            await _client.GetUser(386969677143736330).SendMessageAsync($"Feedback from {Context.User} : {message}");
            await ReplyAsync("Message sent! Thank you! If I choose to reply, I will add you as a friend!");
        }
    }
}
