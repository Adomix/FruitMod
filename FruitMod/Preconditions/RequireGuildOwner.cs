using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace FruitMod.Preconditions
{
    public class RequireGuildOwner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            return context.User.Id == context.Guild.OwnerId
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError($"Command {command.Name} requires the guild owner!"));
        }
    }
}