﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using Discord.Addons.Interactive;
using FruitMod.Interactive.Criteria;

namespace FruitMod.Commands.FunCommands
{
    [RequireContext(ContextType.Guild)]
    public class Fun : InteractiveBase
    {
        private readonly Random _random;
        private readonly DbService _db;
        private static Dictionary<ulong, DateTime> feedback = new Dictionary<ulong, DateTime>();

        public Fun(Random random, DbService db)
        { 
            _random = random;
            _db = db;
        }

        [Command("flip", RunMode = RunMode.Async), Alias("coin flip")]
        [Summary("Bet your Mangos and flip a coin! Usage: flip amount heads/tails")]
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
            var msg = await ReplyAsync("https://media0.giphy.com/media/10bv4HhibS9nZC/giphy.gif");
            await Task.Delay(4000);
            if (odds <= 5 && decider.Equals("heads", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos + bet * 2;
                await msg.ModifyAsync(x => x.Content = $"You won! You have won {bet * 2} Mangos!");
            }
            else if (odds <= 5 && !decider.Equals("heads", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos - bet;
                await msg.ModifyAsync(x => x.Content = $"You Lost! You have lost your bet Mangos!");
            }
            if (odds >= 6 && decider.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos + bet * 2;
                await msg.ModifyAsync(x => x.Content = $"You won! You have won {bet * 2} Mangos!");
            }
            else if (odds >= 6 && !decider.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos - bet;
                await msg.ModifyAsync(x => x.Content = $"You Lost! You have lost your bet Mangos!");
            }
            dbo.UserCurrency[Context.User.Id] = mangos;
            _db.StoreObject(dbo, Context.Guild.Id);
        }


        [Command("roulette", RunMode = RunMode.Async)]
        [Summary("BANG *dead* **Costs 15 mangos** (Chance to make someone lose 1-10 mangos)")]
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
            if (dbo.UserCurrency[Context.User.Id] < 15) { await ReplyAsync($"You do not have enough Mangos to do this! You have {mangos} Mangos!"); return; }
            mangos -= 15;
            dbo.UserCurrency[Context.User.Id] = mangos;
            dbo.UserCurrency[user.Id] -= loss;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"User {user.Username} has been shot! The medics charged them {loss} mangos! They have {dbo.UserCurrency[user.Id]} left!\n You have {mangos} Mangos left!");
        }

        [Command("challenge", RunMode = RunMode.Async)]
        [Summary("Challenges another user! Usage: challenge name amount")]
        public async Task Challenge(int bet, [Remainder] IUser user)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var mangos = dbo.UserCurrency[Context.User.Id];
            if (bet <= 0) { await ReplyAsync($"Bet must be greater than 0! You have {mangos} Mangos!"); return; }
            if (dbo.UserCurrency[Context.User.Id] < bet) { await ReplyAsync($"You do not have enough Mangos to do this! You have {mangos} Mangos!"); return; }
            var p1 = Context.User as SocketGuildUser;
            var p2 = user as SocketGuildUser;

            var criteria = new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureFromUserCriterion(p2.Id));

            if (p1 == p2)
            {
                await ReplyAsync("You can't bet against yourself!");
                return;
            }

            SocketGuildUser winner;

            if (dbo.UserCurrency[p2.Id] < bet)
            {
                await ReplyAsync("Player 2 does not have enough Mangos!");
                return;
            }

            var odds = _random.Next(1, 11);
            var msg = await ReplyAsync($"Player 1 {p1.Nickname ?? p1.Username} has challenged you {p2.Mention} to a duel for {bet} Mangos! Do you accept? y/n (You have 10 seconds)");
            var reply = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(10));
            if (reply.Content.Equals("n", StringComparison.OrdinalIgnoreCase) || reply.Content.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                await msg.ModifyAsync(x => x.Content = "Player 2 has declined!");
                return;
            }
            else if (reply.Content.Equals("y", StringComparison.OrdinalIgnoreCase) || reply.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                await msg.ModifyAsync(x => x.Content = "Hazah! Player 2 accepted! I am suiting up for war!");
                await Task.Delay(2000);
                await msg.ModifyAsync(x => x.Content = "https://www.speakgif.com/wp-content/uploads/2016/07/indiana-jones-duel-animated-gif.gif");
                await Task.Delay(8000);
                if (odds >= 6)
                {
                    winner = p1;
                    await msg.ModifyAsync(x => x.Content = $"{p1.Nickname ?? p1.Username} humilitated {p2.Nickname ?? p2.Username} and took their mangos!");
                    dbo.UserCurrency[p1.Id] += bet;
                    dbo.UserCurrency[p2.Id] -= bet;
                    _db.StoreObject(dbo, Context.Guild.Id);
                    await ReplyAsync($"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserCurrency[winner.Id]} Mangos!");
                }
                else if (odds <= 5)
                {
                    winner = p2;
                    await msg.ModifyAsync(x => x.Content = $"{p2.Nickname ?? p2.Username} humilitated {p1.Nickname ?? p1.Username} and took their mangos!");
                    dbo.UserCurrency[p2.Id] += bet;
                    dbo.UserCurrency[p1.Id] -= bet;
                    _db.StoreObject(dbo, Context.Guild.Id);
                    await ReplyAsync($"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserCurrency[winner.Id]} Mangos!");
                }
            }
            else if (!(reply.Content.Equals("y", StringComparison.OrdinalIgnoreCase) || reply.Content.Equals("n", StringComparison.OrdinalIgnoreCase) || reply.Content.Equals("yes", StringComparison.OrdinalIgnoreCase) || reply.Content.Equals("no", StringComparison.OrdinalIgnoreCase)))
            {
                await msg.ModifyAsync(x => x.Content = "That is not a valid option. Declining. Please use y or n next time.");
                return;
            }
        }
    }
}