using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Objects.DataClasses;
using static FruitMod.Economy.Economy;

namespace FruitMod.Commands.FunCommands
{
    [RequireContext(ContextType.Guild)]
    public class FruitCmds : ModuleBase<FruitModContext>
    {
        private static readonly Dictionary<(ulong, ulong), DateTime> feedback =
            new Dictionary<(ulong, ulong), DateTime>();

        private readonly DbService _db;
        private readonly Random _random;

        public FruitCmds(Random random, DbService db)
        {
            _random = random;
            _db = db;
        }

        [Command("fruits")]
        [Summary("Shows you your fruits!!")]
        public async Task ShowFruits()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {
                var newFruit = new Dictionary<Fruit, int>
                {
                    { Fruit.mangos, 0},
                    {Fruit.watermelons, 0 }
                };

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var total = 0;

            foreach (var fruit in dbo.UserStruct[Context.User.Id].Fruit) total += fruit.Value * fruitValues[fruit.Key];

            await ReplyAsync($"You have:\n{Format.Code(string.Join("\n", dbo.UserStruct[Context.User.Id].Fruit), "ini")}");
        }

        [Command("daily")]
        [Summary("Awards you your daily amount of fruit!")]
        public async Task Daily()
        {
            if (feedback.ContainsKey((Context.Guild.Id, Context.User.Id)) &&
                feedback[(Context.Guild.Id, Context.User.Id)].Date == DateTime.Now.Date)
            {
                await ReplyAsync("You have already redeemed your daily today!");
                return;
            }

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {
                var newFruit = new Dictionary<Fruit, int>
                {
                    { Fruit.mangos, 0},
                    {Fruit.watermelons, 0}
                };
                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            feedback.Add((Context.Guild.Id, Context.User.Id), DateTime.Now);
            var amount = _random.Next(10, 51);
            dbo.UserStruct[Context.User.Id].Fruit[Fruit.mangos] += amount;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have been given {amount} {Fruit.mangos}!");
        }

        [Command("convert")]
        [Summary("Converts 100 mangos to 1 watermelon usage: convert <#OfConversions(optional)>")]
        public async Task FruitConvert(int times = 1)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (dbo.UserStruct[Context.User.Id].Fruit[Fruit.mangos] < 100*times)
            {
                await ReplyAsync("You do not have enough mangos to convert! 1 watermelon is 100 mangos!");
                return;
            }
            dbo.UserStruct[Context.User.Id].Fruit[Fruit.mangos] -= 100 * times;
            dbo.UserStruct[Context.User.Id].Fruit[Fruit.watermelons] += 1 * times;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"{100*times} mangos have been converted to {times} watermelons!");
        }

        [Command("give")]
        [Summary("Gives someone x of your fruits! Usage: give <amount> <fruit> <user>")]
        public async Task GiveFruits(int amount, Fruit fruit, IUser user)
        {
            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("You can not give yourself your fruit!");
                return;
            }

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);

            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {
                await ReplyAsync("You have no fruits!");
                return;
            }

            if (dbo.UserStruct[Context.User.Id].Fruit[fruit] == 0)
            {
                await ReplyAsync($"You do not have any {fruit}!");
                return;
            }

            if (!dbo.UserStruct.ContainsKey(user.Id))
            {
                var newFruit = new Dictionary<Fruit, int>
                {
                    {Fruit.mangos, 0},
                    {Fruit.watermelons, 0}
                };
                dbo.UserStruct.Add(user.Id,
                    new UserStruct { UserId = user.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var invokersFruits = dbo.UserStruct[Context.User.Id].Fruit[fruit];
            var receiversFruits = dbo.UserStruct[user.Id].Fruit[fruit];

            if (amount <= 0)
            {
                await ReplyAsync("Amount must be greater than 0!");
                return;
            }

            if (amount > invokersFruits)
            {
                await ReplyAsync($"You do not have enough {fruit}!");
                return;
            }

            invokersFruits -= amount;
            receiversFruits += amount;

            dbo.UserStruct[Context.User.Id].Fruit[fruit] = invokersFruits;
            dbo.UserStruct[user.Id].Fruit[fruit] = receiversFruits;

            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have successfully given {user} {amount} {fruit}!");
        }

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("WIP (Being reworked)")]
        public async Task Leaderboard()
        {
            await ReplyAsync("WIP");
        }
    }
}