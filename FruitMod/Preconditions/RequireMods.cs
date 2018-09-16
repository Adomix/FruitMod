using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace FruitMod.Preconditions
{
    public class RequireMods : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var db = services.GetService<DbService>();
            var dbo = db.GetById<GuildObjects>(context.Guild.Id);
            var ids = dbo.Settings.ModRoles;

            if (!(context.User is IGuildUser user))
                return Task.FromResult(PreconditionResult.FromError("This command may only be used in a guild."));

            return user.RoleIds.Intersect(ids).Any()
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError("This command requires you to have a moderator role."));
        }
    }
}