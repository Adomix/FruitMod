using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Preconditions;

namespace FruitMod.Commands
{
    [RequireMods(Group = "Moderator")]
    [RequireGuildOwner(Group = "Moderation")]
    [RequireAnyUserPerm(GuildPermission.ManageRoles, GuildPermission.ManageGuild, Group = "Moderation")]
    [RequireOwner(Group = "Moderation")]
    public class ModerationSettings : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public ModerationSettings(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        [Command("prefix add")]
        [Summary("adds a custom prefix, default @bot")]
        public async Task PrefixAdd([Remainder] string prefix)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.Prefixes.Add(prefix + " ");
            _db.StoreObject(dbo, Context.Guild.Id);
            if (dbo.Settings.InfoChannel != null)
                await Context.Guild.GetTextChannel(dbo.Settings.InfoChannel.Value).ModifyAsync(x =>
                    x.Topic =
                        $"Current prefixes: {string.Join(", ", dbo.Settings.Prefixes)} || More Help: https://discord.gg/NVjPVFX");
            await ReplyAsync(
                $"Prefix {prefix} has been added! Current prefixes: {string.Join(", ", dbo.Settings.Prefixes)}");
        }

        [Command("prefix remove")]
        [Summary("adds a custom prefix, default @bot")]
        public async Task PrefixRemove([Remainder] string prefix)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.Prefixes.Remove(prefix + " ");
            _db.StoreObject(dbo, Context.Guild.Id);
            if (dbo.Settings.InfoChannel != null)
                await Context.Guild.GetTextChannel(dbo.Settings.InfoChannel.Value).ModifyAsync(x =>
                    x.Topic =
                        $"Current prefixes: {string.Join(", ", dbo.Settings.Prefixes)} || More Help: https://discord.gg/NVjPVFX");
            await ReplyAsync(
                $"Prefix {prefix} has been removed! Current prefixes: {string.Join(", ", dbo.Settings.Prefixes)}");
        }

        [Command("prefix")]
        [Summary("shows custom prefixes")]
        public async Task PrefixList()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            await ReplyAsync($"Current prefixes: {string.Join(", ", dbo.Settings.Prefixes)}");
        }

        [Command("set logs", RunMode = RunMode.Async)]
        [Summary("sets the channel for the bot information and the current custom prefix")]
        public async Task SetLogs(ITextChannel channel)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var logChannel = channel != null ? Context.Guild.GetChannel(channel.Id) : null;
            if (logChannel != null) dbo.Settings.LogChannel = channel.Id;
            _db.StoreObject(dbo, Context.Guild.Id);
            if (channel != null) await ReplyAsync($"Channel {channel.Name} has been set as the log channel!");
        }

        [Command("set info", RunMode = RunMode.Async)]
        [Summary("sets the channel for bot logging")]
        public async Task SetInfo(ITextChannel channel)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (channel != null)
            {
                var infoChannel = Context.Guild.GetChannel(channel.Id);
                dbo.Settings.InfoChannel = infoChannel.Id;
                _db.StoreObject(dbo, Context.Guild.Id);
                await ReplyAsync($"Channel {channel.Name} has been set as the info channel!");
            }
        }

        [Command("set mute", RunMode = RunMode.Async)]
        [Summary("sets the mute role")]
        public async Task SetMute(IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.MuteRole = role.Id;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"Mute role has been updated to {role.Name}!");
        }

        [Command("role add", RunMode = RunMode.Async)]
        [Summary("Adds a role to the autorole")]
        public async Task AutoRoleAdd([Remainder] IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.AutoRoles.Add(role);
            await ReplyAsync($"New users will now automatically receive {role.Name}!");
        }

        [Command("role del", RunMode = RunMode.Async)]
        [Summary("Deletes a role to the autorole")]
        public async Task AutoRoleDel([Remainder] IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.AutoRoles.Remove(role);
            await ReplyAsync($"New users will no longer automatically receive {role.Name}!");
        }

        [Command("enable vkick", RunMode = RunMode.Async)]
        [Alias("evk")]
        [Summary("Enables votekicking")]
        public async Task EnableVkick()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.VoteSys = true;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("votekick has been successfully enabled!");
        }

        [Command("enable leave", RunMode = RunMode.Async)]
        [Alias("eleave")]
        [Summary("Enables leave logs")]
        public async Task EnableLeave()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (dbo.Settings.LogChannel == null)
            {
                await ReplyAsync("You must first set a log channel. Command: setchannel");
                return;
            }

            dbo.Settings.LeaveSys = true;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Leave logging has been successfully enabled!");
        }

        [Command("enable delete")]
        [Alias("edel")]
        [Summary("enable deleted message logging")]
        public async Task DeleteEnabled()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (dbo.Settings.LogChannel == null)
            {
                await ReplyAsync("You must first set a log channel. Command: setlogs");
                return;
            }

            dbo.Settings.DeleteSys = true;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Deleted message logging enabled!");
        }

        [Command("disable votekick", RunMode = RunMode.Async)]
        [Alias("dvk")]
        [Summary("Disables votekicking")]
        public async Task DisableVkick()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.VoteSys = false;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("votekick has been successfully disabled!");
        }

        [Command("disable leave", RunMode = RunMode.Async)]
        [Alias("dleave")]
        [Summary("Disables leave logs")]
        public async Task DisableLeave()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.LeaveSys = false;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Leave logging has been successfully disabled!");
        }

        [Command("disable delete")]
        [Alias("ddel")]
        [Summary("disable deleted message logging")]
        public async Task DeleteDisabled()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Settings.DeleteSys = false;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Deleted message logging disabled!");
        }
    }
}