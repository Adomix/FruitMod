﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;

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

        [Command("mangos", RunMode = RunMode.Async)]
        [Summary("Shows you your amount of Mangos!")]
        public async Task ShowMangos()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id)) dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            var amount = dbo.UserCurrency[Context.User.Id];
            await ReplyAsync($"You have {amount} Mangos!");
        }

        [Command("daily", RunMode = RunMode.Async)]
        [Summary("Awards you your daily amount of Mangos!")]
        public async Task Daily()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id)) dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            if (feedback.ContainsKey((Context.Guild.Id, Context.User.Id)) &&
                feedback[(Context.Guild.Id, Context.User.Id)].Date == DateTime.Now.Date)
            {
                await ReplyAsync("You have already redeemed your daily today!");
                return;
            }

            feedback.Add((Context.Guild.Id, Context.User.Id), DateTime.Now);
            var amount = _random.Next(10, 51);
            dbo.UserCurrency[Context.User.Id] += amount;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have been given {amount} mangos!");
        }

        [Command("give", RunMode = RunMode.Async)]
        [Summary("Gives someone x of your Mangos! Usage: give 10 user")]
        public async Task GiveMangos(int amount, IUser user)
        {
        
            if(user.Id == Context.User.Id)
            {
                await ReplyAsync("You can not give yourself your mangos!");
                return;
            }
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id)) dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            if (!dbo.UserCurrency.ContainsKey(user.Id)) dbo.UserCurrency.TryAdd(user.Id, 0);
            
            var mangos = dbo.UserCurrency[Context.User.Id];
            var userGive = dbo.UserCurrency[user.Id];

            if (amount <= 0)
            {
                await ReplyAsync($"Amount must be greater than 0! You have {mangos} Mangos!");
                return;
            }

            if (amount > mangos)
            {
                await ReplyAsync($"You do not have enough Mangos! You have {mangos} Mangos!");
                return;
            }
            
            mangos -= amount;
            userGive += amount;
            dbo.UserCurrency[Context.User.Id] = mangos;
            dbo.UserCurrency[user.Id] = userGive;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have successfully given {user} {amount} Mangos!");
        }

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("Shows the top 5 people with mangos")]
        public async Task Leaderboard()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var number = new List<int>();
            var topfive = dbo.UserCurrency.OrderByDescending(x => x.Value);
            var leaders = (from pair in topfive
                let user = Context.Guild.GetUser(pair.Key) as IGuildUser
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
