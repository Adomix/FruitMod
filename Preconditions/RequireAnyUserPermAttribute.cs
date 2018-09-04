using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FruitMod.Preconditions
{
    public class RequireAnyUserPermAttribute : PreconditionAttribute
    {
        // Requires one from multiple perms given
        private GuildPermission[] _perms;

        public RequireAnyUserPermAttribute(params GuildPermission[] perms)
        {
            _perms = perms;
        }
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            IEnumerable<GuildPermission> uperms = ((IGuildUser)context.User).GuildPermissions.ToList();
            return _perms.Intersect(uperms).Any()
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError(""));
        }
    }
}
