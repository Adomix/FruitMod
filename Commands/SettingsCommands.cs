using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Preconditions;

namespace FruitMod.Commands
{
    [RequireAnyUserPermAttribute(GuildPermission.Administrator, GuildPermission.ManageChannels, GuildPermission.BanMembers, GuildPermission.KickMembers,GuildPermission.ManageMessages,
                           GuildPermission.ManageChannels, Group = "settings")]
    [RequireOwner(Group = "settings")]
    public class Settings : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public Settings(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;
        }

        [Command("setlogs", RunMode = RunMode.Async)]
        [Summary("sets the channel for the bot information and the current custom prefix")]
        public async Task SetLogs(ITextChannel channel)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var logChannel = channel != null ? Context.Guild.GetChannel(channel.Id) : null;
            if (logChannel != null) dbo.LogChannel = logChannel.Id;
            _db.StoreObject(dbo, Context.Guild.Id);
            if (channel != null) await ReplyAsync($"Channel {channel.Name} has been set as the log channel!");
        }

        [Command("setinfo", RunMode = RunMode.Async)]
        [Summary("sets the channel for bot logging")]
        public async Task SetInfo(ITextChannel channel)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (channel != null)
            {
                var infoChannel = Context.Guild.GetChannel(channel.Id);
                dbo.InfoChannel = infoChannel.Id;
                _db.StoreObject(dbo, Context.Guild.Id);
                await ReplyAsync($"Channel {channel.Name} has been set as the info channel!");
            }
        }

        [Command("setmute", RunMode = RunMode.Async)]
        [Summary("sets the mute role")]
        public async Task SetMute(IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.MuteRole = role.Id;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"Mute role has been updated to {role.Name}!");
        }

        [Command("evkick", RunMode = RunMode.Async)]
        [Alias("enable votekick")]
        [Summary("Enables votekicking")]
        public async Task EnableVkick()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.VoteSys = true;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("votekick has been successfully enabled!");
        }

        [Command("dvkick", RunMode = RunMode.Async)]
        [Alias("disable votekick")]
        [Summary("Disables votekicking")]
        public async Task DisableVkick()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.VoteSys = false;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("votekick has been successfully disabled!");
        }

        [Command("eleave", RunMode = RunMode.Async)]
        [Alias("enable leave")]
        [Summary("Enables leave logs")]
        public async Task EnableLeave()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (dbo.LogChannel == null)
            {
                await ReplyAsync("You must first set a log channel. Command: setchannel");
                return;
            }

            dbo.LeaveSys = true;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Leave logging has been successfully enabled!");
        }

        [Command("dleave", RunMode = RunMode.Async)]
        [Alias("disable leave")]
        [Summary("Disables leave logs")]
        public async Task DisableLeave()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.LeaveSys = false;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Leave logging has been successfully disabled!");
        }

        [Command("prefix")]
        [Summary("Sets the custom prefix, default @bot")]
        public async Task Prefix([Remainder] string prefix = null)
        {
            if (prefix == null)
            {
                prefix = "_client.CurrentUser.Mention()";
                await ReplyAsync($"Prefix has been updated to: {_client.CurrentUser.Mention} !");
            }

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.Prefix = $"{prefix} ";
            _db.StoreObject(dbo, Context.Guild.Id);
            if (dbo.InfoChannel != null)
                await Context.Guild.GetTextChannel(dbo.InfoChannel.Value).ModifyAsync(x =>
                    x.Topic = $"Current prefix: {prefix} || More Help: https://discord.gg/NVjPVFX");
            await ReplyAsync($"Prefix has been updated to: {prefix} !");
        }

        [Command("ed")]
        [Alias("enable delete")]
        [Summary("enable deleted message logging")]
        public async Task DeleteEnabled()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (dbo.LogChannel == null)
            {
                await ReplyAsync("You must first set a log channel. Command: setlogs");
                return;
            }

            dbo.DeleteSys = true;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Deleted message logging enabled!");
        }

        [Command("dd")]
        [Alias("disable delete")]
        [Summary("disable deleted message logging")]
        public async Task DeleteDisabled()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            dbo.DeleteSys = false;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("Deleted message logging disabled!");
        }

        [Command("reset", RunMode = RunMode.Async)]
        [Summary("Resets all the settings back to default!")]
        public async Task Reset()
        {
            _db.StoreObject(new GuildObjects
            {
                Prefix = "<@467236886616866816> "
            }, Context.Guild.Id);
            await ReplyAsync("Settings have been restored to default!");
        }
    }
}