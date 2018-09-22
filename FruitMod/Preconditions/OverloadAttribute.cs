using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace FruitMod.Preconditions
{
    public class OverloadAttribute : PreconditionAttribute
    {
        // This is for my help command
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}