using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Preconditions;
using FruitMod.Services;

namespace FruitMod.Commands
{
    [RequireAnyUserPermAttribute(GuildPermission.Administrator, GuildPermission.ManageChannels, GuildPermission.ManageMessages, GuildPermission.ManageGuild, GuildPermission.KickMembers, GuildPermission.BanMembers, Group = "admin")]
    [RequireOwner(Group = "admin")]
    [Group("rl")]
    public class Ratelimits : ModuleBase<SocketCommandContext>
    {
        private readonly DbService _db;
        private RatelimitService _rls;
        private readonly DiscordSocketClient _client;

        public Ratelimits(DbService db, RatelimitService rls, DiscordSocketClient client)
        {
            _db = db;
            _rls = rls;
            _client = client;
        }

        [Command("clear")]
        [Summary("removes ratelimits Usage: rl clear <chat>(optional)")]
        public async Task RlClear(string rl = "all")
        {
            if (rl == "all")
            {
                _rls.time = 0;
                _rls.rlb.Add(Context.Channel.Id, false);
                _rls.msgdict.Clear();
            }

            await ReplyAsync("All ratelimits cleared!");
        }

        [Command("chat")]
        [Summary("sets ratelimits for chat. Usage: rl chat <time>(seconds)")]
        public async Task RlChat(int time)
        {
            if (time <= 0)
            {
                await ReplyAsync("Time must be greater than 0!");
                return;
            }
            _rls.rlb.Add(Context.Channel.Id, true);
            _rls.time = time;
            await ReplyAsync($"chat ratelimiting has been turned on! Interval time: {time} second(s)!");
        }
    }
}