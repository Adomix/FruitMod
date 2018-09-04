using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FruitMod.Commands.FeedbackCommands
{
    public class FeedbackCommands : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private static Dictionary<ulong, DateTime> feedback = new Dictionary<ulong, DateTime>();

        public FeedbackCommands(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("feedback")]
        [Summary("Gives the bot owner feedback")]
        public async Task Feedback([Remainder] string message)
        {
            if (feedback.ContainsKey(Context.User.Id) && feedback[Context.User.Id].Date == DateTime.Now.Date) { await ReplyAsync("You have already sent some feedback today!"); return; }
            feedback.Add(Context.User.Id, DateTime.Now);
            await _client.GetUser(386969677143736330).SendMessageAsync($"Feedback from {Context.User} : {message}");
            await ReplyAsync("Thank you! Your feedback has been sent to the bot owner!");
        }
    }
}
