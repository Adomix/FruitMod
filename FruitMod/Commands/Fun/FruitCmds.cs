using System;
using System.Collections.Generic;
using System.Linq;
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

        [Command("fruits", RunMode = RunMode.Async)]
        [Summary("Shows you your fruits!!")]
        public async Task ShowFruits()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {
                var newFruit = new Dictionary<Fruit, int>
                {
                    {Fruit.watermelons, 0},
                    {Fruit.pineapples, 0},
                    {Fruit.mangos, 0}
                };

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                        {UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit});
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var total = 0;

            foreach (var fruit in dbo.UserStruct[Context.User.Id].Fruit) total += fruit.Value * fruitValues[fruit.Key];

            await ReplyAsync(
                $"You have:\n{Format.Code(string.Join("\n", dbo.UserStruct[Context.User.Id].Fruit), "ini")}\nKey:{Format.Code("[watermelons = $1] [pineapples = $2] [mangos = $3]", "ini")}\nTotal: {total}");
        }

        [Command("daily", RunMode = RunMode.Async)]
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
                    {Fruit.watermelons, 0},
                    {Fruit.pineapples, 0},
                    {Fruit.mangos, 0}
                };
                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                        {UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit});
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            feedback.Add((Context.Guild.Id, Context.User.Id), DateTime.Now);
            var amount = _random.Next(10, 51);
            var fruitPicker = _random.Next(0, 101);
            var fruit = Fruit.watermelons;
            if (fruitPicker <= 60)
                fruit = Fruit.watermelons;
            else if (fruitPicker >= 61 && fruitPicker <= 90)
                fruit = Fruit.pineapples;
            else if (fruitPicker >= 91 && fruitPicker <= 100) fruit = Fruit.mangos;
            dbo.UserStruct[Context.User.Id].Fruit[fruit] += amount;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have been given {amount} {fruit}!");
        }

        [Command("give", RunMode = RunMode.Async)]
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
                    {Fruit.watermelons, 0},
                    {Fruit.pineapples, 0},
                    {Fruit.mangos, 0}
                };
                dbo.UserStruct.Add(user.Id,
                    new UserStruct {UserId = user.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit});
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
        [Summary("Shows the top 5 people with mangos")]
        public async Task Leaderboard()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var number = new List<int>();
            var users = new SortedDictionary<IUser, int>();
            foreach (var user in Context.Guild.Users)
                if (dbo.UserStruct.ContainsKey(user.Id))
                    users.Add(user, dbo.UserStruct[user.Id].Fruit[Fruit.mangos]);

            var topfive = users.OrderByDescending(x => x.Value);

            var leaders = (from pair in topfive
                let user = Context.Guild.GetUser(pair.Key.Id) as IGuildUser
                select (user.Nickname ?? user.Username, pair.Value)).ToList();
            if (leaders.Count >= 5)
            {
                leaders.RemoveRange(5, leaders.Count - 5);
                var embed = new EmbedBuilder()
                    .WithTitle("mangos Leaderboard!")
                    .AddField("Top five people:", $"{string.Join("\n", leaders)}")
                    .WithColor(Color.Teal)
                    .Build();
                await ReplyAsync(embed: embed);
            }
            else
            {
                await ReplyAsync("Not enough users with mangos!");
            }
        }
    }
}