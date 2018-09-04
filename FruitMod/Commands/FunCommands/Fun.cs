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

        [Command("flip"), Alias("coin flip")]
        [Summary("Bet your Mangos and flip a coin! Usage: flip <amount> <heads/tails> Ex: flip 10 heads")]
        public async Task Flip(int bet, string decider)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id))
            {
                dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            }
            var mangos = dbo.UserCurrency[Context.User.Id];
            var odds = _random.Next(1, 11);
            if (bet <= 0) { await ReplyAsync($"Bet must be greater than 0! You have {mangos} Mangos!"); return; }
            if (dbo.UserCurrency[Context.User.Id] < bet) { await ReplyAsync($"You do not have enough Mangos to do this! You have {mangos} Mangos!"); return; }
            await ReplyAsync("https://media0.giphy.com/media/10bv4HhibS9nZC/giphy.gif");
            await Task.Delay(4000);
            if (odds <= 5 && decider.Equals("heads", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos + bet * 2;
                await ReplyAsync($"You won! You have won {bet * 2} Mangos!");
            }
            else
            {
                mangos = mangos - bet;
                await ReplyAsync($"You Lost! You have lost your bet Mangos!");
            }
            if (odds <= 6 && decider.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos + bet * 2;
                await ReplyAsync($"You won! You have won {bet * 2} Mangos!");
            }
            else
            {
                mangos = mangos - bet;
                await ReplyAsync($"You Lost! You have lost your bet Mangos!");
            }
            dbo.UserCurrency[Context.User.Id] = mangos;
            _db.StoreObject(dbo, Context.Guild.Id);
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
            var loss = _random.Next(0, 11);
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