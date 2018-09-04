using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace FruitMod.Preconditions
{
    public class RequireNsfw : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (!((SocketTextChannel) context.Channel).IsNsfw)
                return Task.FromResult(PreconditionResult.FromError("Channel is sfw!"));
            if (((SocketTextChannel) context.Channel).IsNsfw) return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("I don't know how I messed this up...."));
        }
    }
}