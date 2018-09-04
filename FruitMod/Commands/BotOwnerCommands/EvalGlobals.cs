using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;

namespace FruitMod.Commands.BotOwnerCommands
{
    public class EvalGlobals
    {
        public SocketCommandContext Context { get; set; }
        public SocketUserMessage Message { get; set; }
        public HttpClient HttpClient { get; set; }
        public IServiceProvider Services { get; set; }
        public DbService db;

        public async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            return await Context.Channel.SendMessageAsync(message, isTTS, embed, options).ConfigureAwait(false);
        }
    }
}