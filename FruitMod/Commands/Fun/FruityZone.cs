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
    public class FruityZone : ModuleBase<FruitModContext>
    {
        private static readonly Dictionary<(ulong, ulong), DateTime> feedback =
            new Dictionary<(ulong, ulong), DateTime>();

        private readonly DbService _db;
        private readonly Random _random;

        public FruityZone(Random random, DbService db)
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
                    { Fruit.Guavas, 0 },
                    { Fruit.Grapes, 0 },
                    { Fruit.Watermelons, 0 },
                    { Fruit.Pineapples, 0 },
                    { Fruit.Mangos, 0 }
                };
                dbo.UserStruct.Add(Context.User.Id, new UserStruct { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            int total = 0;

            foreach (Fruit fruit in dbo.UserStruct[Context.User.Id].Fruit.Keys)
            {
                var value = fruitValues[fruit] * dbo.UserStruct[Context.User.Id].Fruit[fruit];
                total += value;
            }

            var fruits = dbo.UserStruct[Context.User.Id].Fruit;
            await ReplyAsync($"You have {string.Join("\n", fruits)} fruits!\n Your total fruit value is: {total}");
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
                    { Fruit.Guavas, 0 },
                    { Fruit.Grapes, 0 },
                    { Fruit.Watermelons, 0 },
                    { Fruit.Pineapples, 0 },
                    { Fruit.Mangos, 0 }
                };
                dbo.UserStruct.Add(Context.User.Id, new UserStruct { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            feedback.Add((Context.Guild.Id, Context.User.Id), DateTime.Now);
            var amount = _random.Next(10, 51);
            var fruit = _random.Next(0, 4);
            dbo.UserStruct[Context.User.Id].Fruit[(Fruit)fruit] += fruit == (int)Fruit.Mangos ? (int)Math.Round(amount / 2f) : amount;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have been given {amount} of {(Fruit)fruit}!");
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
                    { Fruit.Guavas, 0 },
                    { Fruit.Grapes, 0 },
                    { Fruit.Watermelons, 0 },
                    { Fruit.Pineapples, 0 },
                    { Fruit.Mangos, 0 }
                };
                dbo.UserStruct.Add(user.Id, new UserStruct { UserId = user.Id, Warnings = new Dictionary<int, string>(), Fruit = newFruit });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var invokersFruits = dbo.UserStruct[Context.User.Id].Fruit[fruit];
            var receiversFruits = dbo.UserStruct[user.Id].Fruit[fruit];

            if (amount <= 0)
            {
                await ReplyAsync($"Amount must be greater than 0!");
                return;
            }

            if (amount > invokersFruits)
            {
                await ReplyAsync($"You do not have enough {fruit}!");
                return;
            }

            invokersFruits -= amount;
            receiversFruits += amount;
            // Invoker update
            dbo.UserStruct[Context.User.Id].Fruit.Remove(dbo.UserStruct[Context.User.Id].Fruit.Keys.FirstOrDefault(x => x.Equals(fruit)));
            dbo.UserStruct[Context.User.Id].Fruit.Add(dbo.UserStruct[Context.User.Id].Fruit.Keys.FirstOrDefault(x => x.Equals(fruit)), invokersFruits);
            // Receiver update
            dbo.UserStruct[user.Id].Fruit.Remove(dbo.UserStruct[user.Id].Fruit.Keys.FirstOrDefault(x => x.Equals(fruit)));
            dbo.UserStruct[user.Id].Fruit.Add(dbo.UserStruct[user.Id].Fruit.Keys.FirstOrDefault(x => x.Equals(fruit)), receiversFruits);

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
            {
                if (dbo.UserStruct.ContainsKey(user.Id))
                {
                    users.Add(user, dbo.UserStruct[user.Id].Fruit[Fruit.Mangos]);
                }
            }

            var topfive = users.OrderBy(x => x.Value);

            var leaders = (from pair in topfive
                           let user = Context.Guild.GetUser(pair.Key.Id) as IGuildUser
                           select (user.Nickname ?? user.Username, pair.Value)).ToList();
            if (leaders.Count >= 5)
            {
                leaders.RemoveRange(5, leaders.Count - 5);
                var embed = new EmbedBuilder()
                    .WithTitle("Mangos Leaderboard!")
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
