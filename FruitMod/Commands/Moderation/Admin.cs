using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Preconditions;

namespace FruitMod.Commands
{
    [RequireGuildOwner(Group = "Admin")]
    [RequireAnyUserPerm(GuildPermission.ManageRoles, GuildPermission.ManageGuild, Group = "Admin")]
    [RequireOwner(Group = "Admin")]
    public class Admin : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public Admin(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        [Command("role mod add", RunMode = RunMode.Async)]
        [Summary("Adds a role to the moderator list")]
        public async Task AutoRoleAdd([Remainder] IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.ModRoles.Add(role);
            await ReplyAsync($"New moderator role added! Role: {role.Name}!");
        }

        [Command("role mod del", RunMode = RunMode.Async)]
        [Summary("Deletes a role to the moderator list")]
        public async Task AutoRoleDel([Remainder] IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if ((!dbo.Settings.ModRoles.Contains(role)))
            {
                await ReplyAsync("This is not a moderator role!");
                return;
            }

            dbo.Settings.ModRoles.Remove(role);
            await ReplyAsync($"Moderator role removed! Role: {role.Name}!");
        }

        [Command("reset", RunMode = RunMode.Async)]
        [Summary("Resets all the settings back to default!")]
        public async Task Reset()
        {
            _db.StoreObject(new GuildObjects(), Context.Guild.Id);
            await ReplyAsync("Settings have been restored to default!");
        }
    }
}
