using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;

namespace FruitMod.Commands.FunCommands
{
    [RequireContext(ContextType.Guild)]
    public class Fun : ModuleBase<SocketCommandContext>
    {
        private readonly Random _random;
        private readonly DbService _db;
        private static Dictionary<ulong, DateTime> feedback = new Dictionary<ulong, DateTime>();

        public Fun(Random random, DbService db)
        {
            _random = random;
            _db = db;
        }

        [Command("roulette", RunMode = RunMode.Async)]
        [Summary("BANG *dead* **Costs 10 mangos** (Chance to make someone lose 1-10 mangos)")]
        public async Task React()
        {

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id))
            {
                dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            }

            var list = Context.Guild.Users.Where(x => !x.IsBot).ToList();
            var user = list[_random.Next(1, list.Count())];

            if (!dbo.UserCurrency.ContainsKey(user.Id))
            {
                dbo.UserCurrency.TryAdd(user.Id, 0);
            }
            var loss = _random.Next(0,  11);
            var mangos = dbo.UserCurrency[Context.User.Id];
            if (dbo.UserCurrency[Context.User.Id] < 5) { await ReplyAsync($"You do not have enough Mangos to do this! You have {mangos} Mangos!"); return; }
            mangos -= 5;
            dbo.UserCurrency[Context.User.Id] = mangos;
            dbo.UserCurrency[user.Id] -= loss;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"User {user.Username} has been shot! The medics charged them {loss} mangos! They have {dbo.UserCurrency[user.Id] -= loss} left!\n You have {mangos} Mangos left!");
        }
    }
}