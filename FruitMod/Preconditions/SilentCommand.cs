using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

public class SilentCommand : PreconditionAttribute
{
    private readonly DiscordSocketClient _client;

    public SilentCommand(DiscordSocketClient client)
    {
        _client = client;
    }

    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
        IServiceProvider services)
    {
        if ((await context.Guild.GetUserAsync(context.Client.CurrentUser.Id)).GuildPermissions.Administrator)
        {
            await context.Message.DeleteAsync();
            return PreconditionResult.FromSuccess();
        }
        else
            return PreconditionResult.FromError("Bot may not delete messages!");
    }
}