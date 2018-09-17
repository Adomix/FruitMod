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
            if (!(context.Channel is SocketTextChannel channel)) return Task.FromResult(
                PreconditionResult.FromError("This channel is not a Guild Text Channel"));
            return channel.IsNsfw
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("Channel is sfw!"));
        }
    }
}