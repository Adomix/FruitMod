using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Economy;
using FruitMod.Objects;
using FruitMod.Objects.DataClasses;

namespace FruitMod.Commands.FunCommands
{
    [RequireContext(ContextType.Guild)]
    public class Mangos : ModuleBase<FruitModContext>
    {
        private static readonly Dictionary<(ulong, ulong), DateTime> feedback =
            new Dictionary<(ulong, ulong), DateTime>();

        private readonly DbService _db;
        private readonly Random _random;

        public Mangos(Random random, DbService db)
        {
            _random = random;
            _db = db;
        }

        [Command("fruits", RunMode = RunMode.Async)]
        [Summary("Shows you your fruits!!")]
        public async Task ShowMangos()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {
                var newFruit = new Dictionary<Economy.Fruits, int>
                {
                    { Economy.Fruits.Guavas, 0 },
                    { Economy.Fruits.Grapes, 0 },
                    { Economy.Fruits.Watermelons, 0 },
                    { Economy.Fruits.Pineapples, 0 },
                    { Economy.Fruits.Mangos, 0 }
                };
                dbo.UserStruct.Add(Context.User.Id, new UserStruct { UserId = Context.User.Id, Warnings = 0, Fruits = newFruit });
            }

            var fruits = dbo.UserStruct[Context.User.Id].Fruits;
            await ReplyAsync($"You have {string.Join("\n", fruits)} fruits!");
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
                var newFruit = new Dictionary<Economy.Fruits, int>
                {
                    { Economy.Fruits.Guavas, 0 },
                    { Economy.Fruits.Grapes, 0 },
                    { Economy.Fruits.Watermelons, 0 },
                    { Economy.Fruits.Pineapples, 0 },
                    { Economy.Fruits.Mangos, 0 }
                };
                dbo.UserStruct.Add(Context.User.Id, new UserStruct { UserId = Context.User.Id, Warnings = 0, Fruits = newFruit });
            }

            feedback.Add((Context.Guild.Id, Context.User.Id), DateTime.Now);
            var amount = _random.Next(10, 51);
            var fruit = _random.Next(0, 4);
            dbo.UserStruct[Context.User.Id].Fruits[(Economy.Fruits)fruit] += fruit == (int)Economy.Fruits.Mangos ? (int)Math.Round(amount / 2f) : amount;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have been given {amount} of {(Economy.Fruits)fruit}!");
        }

        [Command("give", RunMode = RunMode.Async)]
        [Summary("Gives someone x of your Mangos! Usage: give <amount> <fruit> <user>")]
        public async Task GiveMangos(int amount, Fruits fruit, IUser user)
        {

            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("You can not give yourself your mangos!");
                return;
            }
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);

            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {
                await ReplyAsync("You have no fruits!");
                return;
            }

            if (dbo.UserStruct[Context.User.Id].Fruits[fruit] == 0)
            {
                await ReplyAsync($"You do not have any {fruit}!");
                return;
            }

            if (!dbo.UserStruct.ContainsKey(user.Id))
            {
                var newFruit = new Dictionary<Economy.Fruits, int>
                {
                    { Economy.Fruits.Guavas, 0 },
                    { Economy.Fruits.Grapes, 0 },
                    { Economy.Fruits.Watermelons, 0 },
                    { Economy.Fruits.Pineapples, 0 },
                    { Economy.Fruits.Mangos, 0 }
                };
                dbo.UserStruct.Add(user.Id, new UserStruct { UserId = user.Id, Warnings = 0, Fruits = newFruit });
            }

            var invokersFruits = dbo.UserStruct[Context.User.Id].Fruits[fruit];
            var receiversFruits = dbo.UserStruct[user.Id].Fruits[fruit];

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
            dbo.UserStruct[Context.User.Id].Fruits.Remove(dbo.UserStruct[Context.User.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(fruit)));
            dbo.UserStruct[Context.User.Id].Fruits.Add(dbo.UserStruct[Context.User.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(fruit)), invokersFruits);
            // Receiver update
            dbo.UserStruct[user.Id].Fruits.Remove(dbo.UserStruct[user.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(fruit)));
            dbo.UserStruct[user.Id].Fruits.Add(dbo.UserStruct[user.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(fruit)), receiversFruits);

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
                    users.Add(user, dbo.UserStruct[user.Id].Fruits.Sum(x => x.Value));
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
                await ReplyAsync("Not enough users are participating!");
            }
        }
    }
}
