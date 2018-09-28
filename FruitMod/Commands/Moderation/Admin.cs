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
    [RequireUserPermission(GuildPermission.ManageGuild, Group = "Admin")]
    [RequireOwner(Group = "Admin")]
    public class Admin : ModuleBase<FruitModContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public Admin(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        [Command("mod add", RunMode = RunMode.Async)]
        [Summary("Adds a role to the moderator list")]
        public async Task ModAdd([Remainder] IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.ModRoles.Add(role.Id);

            if (!dbo.Settings.ModRoles.Contains(role.Id))
            {
                await ReplyAsync("Role failed to add!");
                return;
            }

            await ReplyAsync($"New moderator role added! Role: {role.Name}!");
            _db.StoreObject(dbo, Context.Guild.Id);
        }

        [Command("mod del", RunMode = RunMode.Async)]
        [Summary("Deletes a role to the moderator list")]
        public async Task ModDel([Remainder] IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.Settings.ModRoles.Contains(role.Id))
            {
                await ReplyAsync("This is not a moderator role!");
                return;
            }

            var roles = dbo.Settings.ModRoles;
            roles.Remove(role.Id);
            dbo.Settings.ModRoles = roles;
            await ReplyAsync($"Moderator role removed! Role: {role.Name}!");
            _db.StoreObject(dbo, Context.Guild.Id);
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