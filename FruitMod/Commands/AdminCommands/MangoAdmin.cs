using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Preconditions;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FruitMod.Commands.AdminCommands
{
    [RequireAnyUserPermAttribute(GuildPermission.Administrator, GuildPermission.ManageChannels, GuildPermission.ManageMessages, GuildPermission.ManageGuild, GuildPermission.KickMembers, GuildPermission.BanMembers, Group = "admin")]
    [RequireOwner(Group = "admin")]
    public class MangoAdmin : ModuleBase<SocketCommandContext>
    {
        private readonly DbService _db;
        private readonly DiscordSocketClient _client;

        public MangoAdmin(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        [Command("agive")]
        [Summary("gives x of mangos. Usage: agive 10 user")]
        public async Task AGive(int amount, IUser user)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(user.Id))
            {
                dbo.UserCurrency.TryAdd(user.Id, 0);
            }
            if (user == null) { await ReplyAsync("You must specify a user!"); return; }
            if (amount <= 0) { await ReplyAsync($"Amount must be greater than 0!"); return; }
            var userGive = dbo.UserCurrency[user.Id];
            if (userGive == int.MaxValue) { await ReplyAsync($"User is already at the max amount of Mangos! {int.MaxValue}"); }
            if (amount >= int.MaxValue) { await ReplyAsync($"No overflowing! Will set the user to the max!"); amount = int.MaxValue - userGive; }
            userGive += amount;
            dbo.UserCurrency[user.Id] = userGive;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have successfully given {user} {amount} Mangos!");
        }

        [Command("agive")]
        [Summary("gives x of mangos. Usage: agive 10 @everyone")]
        public async Task AGive(int amount, IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (role == null) { await ReplyAsync("You must specify a role!"); return; }
            if (amount <= 0) { await ReplyAsync($"Amount must be greater than 0!"); return; }
            foreach (IUser user in (role as SocketRole).Members)
            {
                if (!dbo.UserCurrency.ContainsKey(user.Id))
                {
                    dbo.UserCurrency.TryAdd(user.Id, 0);
                }
                if (amount >= int.MaxValue)
                {
                    amount = int.MaxValue - dbo.UserCurrency[user.Id];
                }
                if (dbo.UserCurrency[user.Id] >= int.MaxValue)
                {
                    amount = int.MaxValue - dbo.UserCurrency[user.Id];
                }
                dbo.UserCurrency[user.Id] = dbo.UserCurrency[user.Id] + amount;
            }
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have successfully given `{role.Name}` {amount} Mangos!");
        }

        [Command("mangor", RunMode = RunMode.Async)]
        [Summary("Resets everyones mangos")]
        public async Task MangoR()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var currency = new ConcurrentDictionary<ulong, int>();
            foreach (ulong id in dbo.UserCurrency.Keys)
            {
                currency.TryAdd(id, 0);
            }
            dbo.UserCurrency = currency;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("All the Mangos have been eaten!");
        }

        [Command("mangor", RunMode = RunMode.Async)]
        [Summary("Resets everyones mangos. Usage: mangor user")]
        public async Task MangoR(IUser user)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id))
            {
                dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            }
            dbo.UserCurrency[user.Id] = 0;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"User's {user} Mangos have been eaten!");
        }

        [Command("mangorb", RunMode = RunMode.Async)]
        [Summary("Removes all bots added during mango admin operations")]
        public async Task Mangorb()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var y = new ConcurrentDictionary<ulong, int>();
            foreach (var x in dbo.UserCurrency)
            {
                var user = Context.Guild.GetUser(x.Key);
                if (!user.IsBot) y.TryAdd(x.Key, x.Value);
            }
            dbo.UserCurrency = y;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("All bots removed!");
        }
    }
}
