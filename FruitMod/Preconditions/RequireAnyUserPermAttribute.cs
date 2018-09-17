using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace FruitMod.Preconditions
{
    public class RequireAnyUserPermAttribute : PreconditionAttribute
    {
        // Requires one from multiple perms given
        private readonly GuildPermission[] _perms;

        public RequireAnyUserPermAttribute(params GuildPermission[] perms)
        {
            _perms = perms;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            IEnumerable<GuildPermission> uperms = (context.User as IGuildUser)?.GuildPermissions.ToList();
            return (!(uperms is null) || _perms.Intersect(uperms).Any())
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError(""));
        }
    }
}