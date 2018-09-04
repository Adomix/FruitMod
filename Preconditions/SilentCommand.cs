using System;
using System.Threading.Tasks;
using Discord.Commands;

public class SilentCommand : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
        IServiceProvider services)
    {
        await context.Message.DeleteAsync();
        return PreconditionResult.FromSuccess();
    }
}