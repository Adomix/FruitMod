﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Interactive.Criteria;
using FruitMod.Objects;
using FruitMod.Objects.DataClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FruitMod.Commands.FunCommands
{
    [RequireContext(ContextType.Guild)]
    public class Fun : InteractiveBase
    {
        private static Dictionary<ulong, DateTime> feedback = new Dictionary<ulong, DateTime>();
        private readonly DbService _db;
        private readonly HttpClient _http;
        private readonly Random _random;

        public Fun(Random random, DbService db, HttpClient http)
        {
            _random = random;
            _db = db;
            _http = http;
        }

        [Command("flip", RunMode = RunMode.Async)]
        [Alias("coin flip")]
        [Summary(
            "Bet your Fruit and flip a coin! Usage: flip <amount> <heads/tails>")]
        public async Task Flip(int bet, string decider)
        {
            if (bet <= 0)
            {
                await ReplyAsync("Bet must be greater than 0!");
                return;
            }

            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);

            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Mangos = 0 });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var invokersFruit = dbo.UserStruct[Context.User.Id].Mangos;
            var odds = _random.Next(1, 11);

            if (invokersFruit < bet)
            {
                await ReplyAsync($"You do not have enough mangos to do this! You have {invokersFruit} mangos!");
                return;
            }

            if (!(decider.Equals("heads", StringComparison.OrdinalIgnoreCase) ||
                  decider.Equals("tails", StringComparison.OrdinalIgnoreCase))) return;

            var msg = await ReplyAsync("https://media0.giphy.com/media/10bv4HhibS9nZC/giphy.gif");
            await Task.Delay(4000);

            var headsWin = odds < 6;
            var playerWins = headsWin && decider.Equals("heads", StringComparison.OrdinalIgnoreCase) ||
                             !headsWin && decider.Equals("tails", StringComparison.OrdinalIgnoreCase);


            if (invokersFruit + (int)Math.Round(1.8 * bet) >= int.MaxValue)
                invokersFruit += playerWins ? int.MaxValue : -bet;
            else
                invokersFruit += playerWins ? (int)Math.Round(1.8 * bet) : -bet;

            dbo.UserStruct[Context.User.Id].Mangos = invokersFruit;

            _db.StoreObject(dbo, Context.Guild.Id);

            if (!(dbo.UserStruct[Context.User.Id].Mangos == invokersFruit))
            {
                await ReplyAsync("Database busy! Try again!");
                return;
            }

            if (playerWins)
                await msg.ModifyAsync(x =>
                    x.Content =
                        $"{Context.GuildUser.Nickname ?? Context.GuildUser.Username} you won! You have won {(int)Math.Round(1.2 * bet)} mangos!");
            else
                await msg.ModifyAsync(x =>
                    x.Content =
                        $"{Context.GuildUser.Nickname ?? Context.GuildUser.Username} you Lost! You have lost your {bet} mangos!");
        }

        [Command("roulette", RunMode = RunMode.Async)]
        [Summary("BANG *dead* **5 mangos** (This will make someones lose 0-15 of a random fruit!)")]
        public async Task React()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);

            if (!dbo.UserStruct.ContainsKey(Context.User.Id))
            {

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Mangos = 0 });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var list = Context.Guild.Users.Where(x => !x.IsBot).ToList();
            var user = list[_random.Next(1, list.Count())];

            if (!dbo.UserStruct.ContainsKey(user.Id))
            {

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = user.Id, Warnings = new Dictionary<int, string>(), Mangos = 0 });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var loss = _random.Next(0, 11);
            var invokersFruit = dbo.UserStruct[Context.User.Id].Mangos;
            if (invokersFruit < 5)
            {
                await ReplyAsync($"You do not have enough mangos to do this! You have {invokersFruit} mangos!");
                return;
            }

            // Removing the 5 mangos from the user
            invokersFruit -= 5;
            dbo.UserStruct[Context.User.Id].Mangos = invokersFruit;
            // Removing the random amount of fruit from the target
            dbo.UserStruct[user.Id].Mangos -= loss;

            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync(
                $"User {user.Nickname ?? user.Username} has been shot! The medics charged them {loss} mangos! They have {dbo.UserStruct[user.Id].Mangos} left!\n You have {dbo.UserStruct[Context.User.Id].Mangos} mangos left!");
        }

        [Command("challenge", RunMode = RunMode.Async)]
        [Summary("Challenges another user! Usage: challenge <bet> <user>")]
        public async Task Challenge(int bet, [Remainder] IUser user)
        {
            if (bet <= 0)
            {
                await ReplyAsync("Bet must be greater than 0!");
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

                dbo.UserStruct.Add(Context.User.Id,
                    new UserStruct
                    { UserId = Context.User.Id, Warnings = new Dictionary<int, string>(), Mangos = 0 });
                _db.StoreObject(dbo, Context.Guild.Id);
            }

            var invokersFruit = dbo.UserStruct[Context.User.Id].Mangos;
            var receiversFruit = dbo.UserStruct[user.Id].Mangos;

            var p1 = Context.GuildUser;
            var p2 = user as SocketGuildUser;

            if (invokersFruit < bet)
            {
                await ReplyAsync($"You do not have enough Fruit to do this! You have {invokersFruit} mangos!");
                return;
            }

            if (p1 == p2)
            {
                await ReplyAsync("You can't bet against yourself!");
                return;
            }

            if (receiversFruit < bet)
            {
                await ReplyAsync($"Player 2 does not have enough mangos!");
                return;
            }

            var criteria = new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureFromUserCriterion(p2.Id));

            SocketGuildUser winner;
            var odds = _random.Next(1, 11);

            var msg = await ReplyAsync(
                $"Player 1 {p1.Nickname ?? p1.Username} has challenged you {p2.Mention} to a duel for {bet} mangos! Do you accept? y/n (You have 10 seconds)");

            var reply = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(10));

            var content = reply.Content;

            if (content.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                content.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                await msg.ModifyAsync(x => x.Content = "Player 2 has declined!");
            }
            else if (content.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                     content.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                await msg.ModifyAsync(x => x.Content = "Hazah! Player 2 accepted! I am suiting up for war!");
                await Task.Delay(2000);
                await msg.ModifyAsync(x =>
                    x.Content =
                        "https://www.speakgif.com/wp-content/uploads/2016/07/indiana-jones-duel-animated-gif.gif");
                await Task.Delay(8000);

                if (odds >= 6)
                {
                    winner = p1;

                    await msg.ModifyAsync(x =>
                        x.Content =
                            $"{p1.Nickname ?? p1.Username} humilitated {p2.Nickname ?? p2.Username} and took their mangos!");

                    dbo.UserStruct[Context.User.Id].Mangos += bet;
                    dbo.UserStruct[user.Id].Mangos -= bet;

                    _db.StoreObject(dbo, Context.Guild.Id);

                    await ReplyAsync(
                        $"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserStruct[winner.Id].Mangos} mangos!");
                }
                else if (odds <= 5)
                {
                    winner = p2;

                    await msg.ModifyAsync(x =>
                        x.Content =
                            $"{p2.Nickname ?? p2.Username} humilitated {p1.Nickname ?? p1.Username} and took their mangos!");

                    dbo.UserStruct[user.Id].Mangos += bet;
                    dbo.UserStruct[Context.User.Id].Mangos -= bet;

                    _db.StoreObject(dbo, Context.Guild.Id);

                    await ReplyAsync(
                        $"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserStruct[winner.Id].Mangos} mangos!");
                }
            }
            else
            {
                await msg.ModifyAsync(x =>
                    x.Content = "That is not a valid option. Declining. Please use (y / yes) or (n / no) next time.");
            }
        }

        [Overload]
        [Command("challenge", RunMode = RunMode.Async)]
        [Summary("Challenges another user! Usage: challenge <bet> <fruit> <user>")]
        public async Task Challenge(int bet, [Remainder] string user)
        {
            await Challenge(bet,
                Context.Guild.Users.FirstOrDefault(x => x.Username.Contains(user) || x.Nickname.Contains(user)));
        }

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
            using (var obj = await _http.GetAsync($"https://api.wolframalpha.com/v2/query?input={input}&format=image,plaintext&output=JSON&appid={ConfigurationManager.AppSettings["wolfram"]}"))
            {
                var json = JsonConvert.DeserializeObject<WolframStuff>(await obj.Content.ReadAsStringAsync());
                var spod = json.QueryResult.Pods.SelectMany(x => x.Subpods).Select(y => y.Plaintext).Take(3);
                await ReplyAsync($"Returned results:\n{string.Join("\n", spod)}");
            }
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

        public class WolframStuff
        {
            [JsonProperty("queryresult")]
            public Queryresult QueryResult { get; set; }

            public partial class Queryresult
            {
                [JsonProperty("pods")]
                public Pod[] Pods { get; set; }
            }

            public partial class Pod
            {
                [JsonProperty("subpods")]
                public Subpod[] Subpods { get; set; }
            }

            public partial class Subpod
            {
                [JsonProperty("plaintext")]
                public string Plaintext { get; set; }
            }
        }

    }
}