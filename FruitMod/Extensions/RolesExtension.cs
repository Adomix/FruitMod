using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FruitMod.Extensions
{
    public static class RolesExtension
    {
        public static async Task AddRolesAsync(this SocketGuildUser user, IEnumerable<IRole> roles)
        {
            foreach (var role in roles) await user.AddRoleAsync(role);
        }
    }
}