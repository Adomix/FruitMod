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
using FruitMod.Interactive.Criteria;
using FruitMod.Objects;
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
        [Summary("Bet your Mangos and flip a coin! Usage: flip amount heads/tails")]
        public async Task Flip(int bet, string decider)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id)) dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            var mangos = dbo.UserCurrency[Context.User.Id];
            var odds = _random.Next(1, 11);
            if (bet <= 0)
            {
                await ReplyAsync($"Bet must be greater than 0! You have {mangos} Mangos!");
                return;
            }

            if (dbo.UserCurrency[Context.User.Id] < bet)
            {
                await ReplyAsync($"You do not have enough Mangos to do this! You have {mangos} Mangos!");
                return;
            }

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
                await msg.ModifyAsync(x => x.Content = "You Lost! You have lost your bet Mangos!");
            }

            if (odds >= 6 && decider.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos + bet * 2;
                await msg.ModifyAsync(x => x.Content = $"You won! You have won {bet * 2} Mangos!");
            }
            else if (odds >= 6 && !decider.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                mangos = mangos - bet;
                await msg.ModifyAsync(x => x.Content = "You Lost! You have lost your bet Mangos!");
            }

            dbo.UserCurrency[Context.User.Id] = mangos;
            _db.StoreObject(dbo, Context.Guild.Id);
        }


        [Command("roulette", RunMode = RunMode.Async)]
        [Summary("BANG *dead* **Costs 15 mangos** (Chance to make someone lose 1-10 mangos)")]
        public async Task React()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id)) dbo.UserCurrency.TryAdd(Context.User.Id, 0);

            var list = Context.Guild.Users.Where(x => !x.IsBot).ToList();
            var user = list[_random.Next(1, list.Count())];

            if (!dbo.UserCurrency.ContainsKey(user.Id)) dbo.UserCurrency.TryAdd(user.Id, 0);
            var loss = _random.Next(0, 11);
            var mangos = dbo.UserCurrency[Context.User.Id];
            if (dbo.UserCurrency[Context.User.Id] < 15)
            {
                await ReplyAsync($"You do not have enough Mangos to do this! You have {mangos} Mangos!");
                return;
            }

            mangos -= 15;
            dbo.UserCurrency[Context.User.Id] = mangos;
            dbo.UserCurrency[user.Id] -= loss;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync(
                $"User {user.Username} has been shot! The medics charged them {loss} mangos! They have {dbo.UserCurrency[user.Id]} left!\n You have {mangos} Mangos left!");
        }

        [Command("challenge", RunMode = RunMode.Async)]
        [Summary("Challenges another user! Usage: challenge name amount")]
        public async Task Challenge(int bet, [Remainder] IUser user)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var mangos = dbo.UserCurrency[Context.User.Id];
            if (bet <= 0)
            {
                await ReplyAsync($"Bet must be greater than 0! You have {mangos} Mangos!");
                return;
            }

            if (dbo.UserCurrency[Context.User.Id] < bet)
            {
                await ReplyAsync($"You do not have enough Mangos to do this! You have {mangos} Mangos!");
                return;
            }

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
            var msg = await ReplyAsync(
                $"Player 1 {p1.Nickname ?? p1.Username} has challenged you {p2.Mention} to a duel for {bet} Mangos! Do you accept? y/n (You have 10 seconds)");
            var reply = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(10));
            if (reply.Content.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                reply.Content.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                await msg.ModifyAsync(x => x.Content = "Player 2 has declined!");
            }
            else if (reply.Content.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                     reply.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
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
                    dbo.UserCurrency[p1.Id] += bet;
                    dbo.UserCurrency[p2.Id] -= bet;
                    _db.StoreObject(dbo, Context.Guild.Id);
                    await ReplyAsync(
                        $"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserCurrency[winner.Id]} Mangos!");
                }
                else if (odds <= 5)
                {
                    winner = p2;
                    await msg.ModifyAsync(x =>
                        x.Content =
                            $"{p2.Nickname ?? p2.Username} humilitated {p1.Nickname ?? p1.Username} and took their mangos!");
                    dbo.UserCurrency[p2.Id] += bet;
                    dbo.UserCurrency[p1.Id] -= bet;
                    _db.StoreObject(dbo, Context.Guild.Id);
                    await ReplyAsync(
                        $"{winner.Nickname ?? winner.Username} congrats on the victory! You now have {dbo.UserCurrency[winner.Id]} Mangos!");
                }
            }
            else if (!(reply.Content.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                       reply.Content.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                       reply.Content.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                       reply.Content.Equals("no", StringComparison.OrdinalIgnoreCase)))
            {
                await msg.ModifyAsync(x =>
                    x.Content = "That is not a valid option. Declining. Please use y or n next time.");
            }
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
            var color = new List<string> {"FF0000", "00A6FF", "AA00FF", "26C200"};
            var colorn = _random.Next(color.Count + 1);
            var cpick = color[colorn];
            var response = await _http.GetStringAsync(
                $"https://img4me.p.mashape.com/?bcolor=%23{cpick}&fcolor=000000&font=trebuchet&size=12&text={text}&type=png");
            await ReplyAsync(response);
        }
    }
}