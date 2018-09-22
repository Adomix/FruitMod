using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Economy;
using FruitMod.Interactive.Criteria;
using FruitMod.Objects;
using FruitMod.Objects.DataClasses;
using Newtonsoft.Json.Linq;

namespace FruitMod.Commands.FunCommands
{
    [RequireContext(ContextType.Guild)]
    public partial class Fun : InteractiveBase
    {
        private static Dictionary<ulong, DateTime> feedback = new Dictionary<ulong, DateTime>();
        private readonly DbService _db;
        private readonly Queue<(string suit, string card, int value)> _ddeck;
        private readonly Queue<(string suit, string card, int value)> _deck;
        private readonly HttpClient _http;
        private readonly Random _random;

        public Fun(Random random, DbService db, HttpClient http)
        {
            _random = random;
            _db = db;
            _http = http;
            _deck = new Queue<(string, string, int)>(
                (from suit in _suits from card in _cards select (suit, card.Key, card.Value)).OrderBy(_ =>
                    _random.Next()));
            _ddeck = new Queue<(string, string, int)>(
                (from suit in _suits from card in _cards select (suit, card.Key, card.Value)).OrderBy(_ =>
                    _random.Next()));
        }

        [Command("flip", RunMode = RunMode.Async)]
        [Alias("coin flip")]
        [Summary("Bet your fruits and flip a coin! Usage: flip <amount> <heads/tails> <fruit(option, default is grapes)>")]
        public async Task Flip(int bet, string decider, Fruits fruit = Fruits.Grapes)
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

            var invokersFruit = dbo.UserStruct[Context.User.Id].Fruits[fruit];
            var odds = _random.Next(1, 11);

            if (bet <= 0)
            {
                await ReplyAsync($"Bet must be greater than 0! You have {invokersFruit} {fruit}!");
                return;
            }

            if (invokersFruit < bet)
            {
                await ReplyAsync($"You do not have enough Mangos to do this! You have {invokersFruit} {fruit}!");
                return;
            }

            if (!(decider.Equals("heads", StringComparison.OrdinalIgnoreCase) ||
                decider.Equals("tails", StringComparison.OrdinalIgnoreCase))) return;

            var msg = await ReplyAsync("https://media0.giphy.com/media/10bv4HhibS9nZC/giphy.gif");
            await Task.Delay(4000);

            var headsWin = odds < 6;
            var playerWins = headsWin && decider.Equals("heads", StringComparison.OrdinalIgnoreCase) ||
                             !headsWin && decider.Equals("tails", StringComparison.OrdinalIgnoreCase);

            if (invokersFruit + (int)Math.Round(1.2 * bet) >= int.MaxValue)
            {
                await ReplyAsync("You are going to go over the max! Giving you the difference!");
                var dif = int.MaxValue - (int)Math.Round(1.2 * bet);
                invokersFruit += playerWins ? dif : -bet;
            }
            else
            {
                invokersFruit += playerWins ? (int)Math.Round(1.2 * bet) : -bet;
            }

            dbo.UserStruct[Context.User.Id].Fruits.Remove(dbo.UserStruct[Context.User.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(fruit)));
            dbo.UserStruct[Context.User.Id].Fruits.Add(dbo.UserStruct[Context.User.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(fruit)), invokersFruit);

            _db.StoreObject(dbo, Context.Guild.Id);

            if (!(dbo.UserStruct[Context.User.Id].Fruits[fruit] == invokersFruit))
            {
                await ReplyAsync($"Database busy! Try again!");
            }

            if (playerWins)
                await msg.ModifyAsync(x => x.Content = $"{Context.GuildUser.Nickname ?? Context.GuildUser.Username} you won! You have won {bet * 2} {fruit}!");
            else
                await msg.ModifyAsync(x => x.Content = $"{Context.GuildUser.Nickname ?? Context.GuildUser.Username} you Lost! You have lost your bet {fruit}!");
        }

        [Command("roulette", RunMode = RunMode.Async)]
        [Summary("BANG *dead* **5 mangos** (This will make someones lose 0-15 of a random fruit!)")]
        public async Task React()
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

            var list = Context.Guild.Users.Where(x => !x.IsBot).ToList();
            var user = list[_random.Next(1, list.Count())];

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
            var loss = _random.Next(0, 11);
            var invokersFruit = dbo.UserStruct[Context.User.Id].Fruits[Fruits.Mangos];
            if (dbo.UserStruct[Context.User.Id].Fruits[Fruits.Mangos] < 5)
            {
                await ReplyAsync($"You do not have enough Mangos to do this! You have {invokersFruit} Mangos!");
                return;
            }

            // Removing the 5 Mangos from the user
            invokersFruit -= 5;
            dbo.UserStruct[Context.User.Id].Fruits.Remove(dbo.UserStruct[Context.User.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(Fruits.Mangos)));
            dbo.UserStruct[Context.User.Id].Fruits.Add(dbo.UserStruct[Context.User.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(Fruits.Mangos)), invokersFruit);
            // Removing the random amount of fruit from the target
            var fruit = _random.Next(0, 4);
            Fruits[] fruitTypes = { Fruits.Grapes, Fruits.Guavas, Fruits.Pineapples, Fruits.Watermelons };
            var lostFruit = dbo.UserStruct[user.Id].Fruits[fruitTypes[fruit]] -= loss;
            dbo.UserStruct[user.Id].Fruits.Remove(fruitTypes[fruit]);
            dbo.UserStruct[user.Id].Fruits.Add(dbo.UserStruct[user.Id].Fruits.Keys.FirstOrDefault(x => x.Equals(Fruits.Mangos)), loss);

            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"User {user.Nickname ?? user.Username} has been shot! The medics charged them {loss} {fruitTypes[fruit]}! They have {dbo.UserStruct[user.Id].Fruits[fruitTypes[fruit]]} left!\n You have {dbo.UserStruct[Context.User.Id].Fruits[Fruits.Mangos]} Mangos left!");
        }

        [Command("challenge", RunMode = RunMode.Async)]
        [Summary("Challenges another user! Usage: challenge <bet> <fruit> <user>")]
        public async Task Challenge(int bet, Fruits fruit, [Remainder] IUser user)
        {
            if (bet <= 0)
            {
                await ReplyAsync($"Bet must be greater than 0!");
                return;
            }

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);

            if (!dbo.UserStruct.ContainsKey(user.Id))
            {
                await ReplyAsync("User does not have any fruit!");
                return;
            }

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

            var invokersFruit = dbo.UserStruct[Context.User.Id].Fruits[fruit];
            var receiversFruit = dbo.UserStruct[user.Id].Fruits[fruit];

            var p1 = Context.GuildUser;
            var p2 = user as SocketGuildUser;

            if (invokersFruit < bet)
            {
                await ReplyAsync($"You do not have enough Mangos to do this! You have {invokersFruit} {fruit}!");
                return;
            }

            if (p1 == p2)
            {
                await ReplyAsync("You can't bet against yourself!");
                return;
            }

            if (receiversFruit < bet)
            {
                await ReplyAsync("Player 2 does not have enough Mangos!");
                return;
            }

            var criteria = new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureFromUserCriterion(p2.Id));

            SocketGuildUser winner;
            var odds = _random.Next(1, 11);

            var msg = await ReplyAsync($"Player 1 {p1.Nickname ?? p1.Username} has challenged you {p2.Mention} to a duel for {bet} Mangos! Do you accept? y/n (You have 10 seconds)");

            var reply = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(10));

            var content = reply.Content;

            if (content.Equals("n", StringComparison.OrdinalIgnoreCase) || content.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                await msg.ModifyAsync(x => x.Content = "Player 2 has declined!");
            }
            else if (content.Equals("y", StringComparison.OrdinalIgnoreCase) || content.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                await msg.ModifyAsync(x => x.Content = "Hazah! Player 2 accepted! I am suiting up for war!");
                await Task.Delay(2000);
                await msg.ModifyAsync(x => x.Content = "https://www.speakgif.com/wp-content/uploads/2016/07/indiana-jones-duel-animated-gif.gif");
                await Task.Delay(8000);

                if (odds >= 6)
                {
                    winner = p1;

                    await msg.ModifyAsync(x => x.Content = $"{p1.Nickname ?? p1.Username} humilitated {p2.Nickname ?? p2.Username} and took their {fruit}!");

                    dbo.UserStruct[Context.User.Id].Fruits[fruit] += bet;
                    dbo.UserStruct[user.Id].Fruits[fruit] -= bet;

                    _db.StoreObject(dbo, Context.Guild.Id);

                    await ReplyAsync($"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserStruct[winner.Id].Fruits[fruit]} {fruit}!");
                }
                else if (odds <= 5)
                {
                    winner = p2;

                    await msg.ModifyAsync(x => x.Content = $"{p2.Nickname ?? p2.Username} humilitated {p1.Nickname ?? p1.Username} and took their mangos!");

                    dbo.UserStruct[user.Id].Fruits[fruit] += bet;
                    dbo.UserStruct[Context.User.Id].Fruits[fruit] -= bet;

                    _db.StoreObject(dbo, Context.Guild.Id);

                    await ReplyAsync($"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserStruct[winner.Id].Fruits[fruit]} Mangos!");
                }
            }
            else
            {
                await msg.ModifyAsync(x => x.Content = "That is not a valid option. Declining. Please use (y / yes) or (n / no) next time.");
            }
        }

        [Command("challenge", RunMode = RunMode.Async)]
        [Summary("Challenges another user! Usage: challenge <bet> <fruit> <user>")]
        public async Task Challenge(int bet, Fruits fruit, [Remainder] string user)
            => await Challenge(bet, fruit, Context.Guild.Users.FirstOrDefault(x => x.Username.Contains(user) || x.Nickname.Contains(user)));

        [Command("cat", RunMode = RunMode.Async)]
        [Alias("cfact")]
        [Summary("gives a cat fact!")]
        public async Task CFact()
        {
            var jResponse =
                JObject.Parse(await _http.GetStringAsync("https://cat-fact.herokuapp.com/facts/random?amount=1"));
            await ReplyAsync(jResponse["text"].ToString());
        }

        [Command("ud", RunMode = RunMode.Async)]
        [Summary("Gives an urban dictionary definition")]
        public async Task Ud([Remainder] string word)
        {
            var jResponse =
                JObject.Parse(await _http.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={word}"));
            await ReplyAsync(jResponse["list"][0]["definition"].ToString());
        }

        [Command("wa", RunMode = RunMode.Async)]
        [Alias("wolfram")]
        [Summary("Returns a wolfram alpha output")]
        public async Task Wa([Remainder] string input)
        {
            var jResponse = JObject.Parse(await _http.GetStringAsync(
                $"https://api.wolframalpha.com/v2/query?input={input}&format=image,plaintext&output=JSON&appid=" +
                ConfigurationManager.AppSettings["wolfram"]));
            await ReplyAsync(jResponse["queryresult"]["pods"][2]["subpods"][0]["plaintext"].ToString());
        }

        [Command("tti", RunMode = RunMode.Async)]
        [Summary("Turns text into an image")]
        public async Task Tti([Remainder] string text)
        {
            _http.DefaultRequestHeaders.Add("X-Mashape-Key", ConfigurationManager.AppSettings["mashape"]);
            var color = new List<string> { "FF0000", "00A6FF", "AA00FF", "26C200" };
            var colorn = _random.Next(color.Count + 1);
            var cpick = color[colorn];
            var response = await _http.GetStringAsync(
                $"https://img4me.p.mashape.com/?bcolor=%23{cpick}&fcolor=000000&font=trebuchet&size=12&text={text}&type=png");
            await ReplyAsync(response);
        }
    }
}