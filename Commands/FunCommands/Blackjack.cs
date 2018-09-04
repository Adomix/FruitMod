using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;
using Discord.Addons.Interactive;

namespace FruitMod.Commands.FunCommands
{
    public class Blackjack : InteractiveBase
    {
        private readonly DbService _db;
        private readonly Random _randomizer;
        public bool blackjackResult;
        private readonly Queue<(string suit, string card, int value)> _deck;
        private readonly Queue<(string suit, string card, int value)> _ddeck;

        public Blackjack(Random randomizer, DbService db)
        {
            _randomizer = randomizer;
            _db = db;
            _deck = new Queue<(string, string, int)>((from suit in _suits from card in _cards select (suit, card.Key, card.Value)).OrderBy(_ => _randomizer.Next()));
            _ddeck = new Queue<(string, string, int)>((from suit in _suits from card in _cards select (suit, card.Key, card.Value)).OrderBy(_ => _randomizer.Next()));
        }

        private readonly IReadOnlyDictionary<string, int> _cards = new Dictionary<string, int> { { "ace", 1 }, { "two", 2 },{ "three", 3 }, { "four", 4 },{ "five", 5 },{ "six", 6 },{ "seven", 7 },
            { "eight", 8 },{ "nine", 9 },{ "ten", 10 },{ "jack", 10 },{ "queen", 10 },{ "king", 10 }};

        private readonly IReadOnlyCollection<string> _suits = new[] { "❤", "♦", "♣", "♠" };

        private ConcurrentDictionary<KeyValuePair<string, int>, int> duplicate = new ConcurrentDictionary<KeyValuePair<string, int>, int>();
        private ConcurrentDictionary<KeyValuePair<string, int>, int> dduplicate = new ConcurrentDictionary<KeyValuePair<string, int>, int>();
        private ConcurrentDictionary<ulong, Queue<(string suit, string card, int value)>> userData = new ConcurrentDictionary<ulong, Queue<(string suit, string card, int value)>>();
        private ConcurrentDictionary<ulong, Queue<(string suit, string card, int value)>> userDData = new ConcurrentDictionary<ulong, Queue<(string suit, string card, int value)>>();
        private List<int> values = new List<int>();
        private List<int> dvalues = new List<int>();
        private List<int> dtvalues = new List<int>();
        public Embed MyEmbed(ulong id)
        {
            var deck = userData[id];
            var ddeck = userDData[id];
            var card = deck.Dequeue();
            var suit = card.suit;
            var value = card.value;
            duplicate.TryAdd(new KeyValuePair<string, int>(suit, value), 1);
            var card2 = deck.Dequeue();
            var suit2 = card2.suit;
            var value2 = card2.value;
            values.Add(value);
            values.Add(value2);
            duplicate.TryAdd(new KeyValuePair<string, int>(suit2, value2), 2);
            var dcard = ddeck.Dequeue();
            var dsuit = dcard.suit;
            var dvalue = dcard.value;
            var dcard2 = ddeck.Dequeue();
            var dsuit2 = dcard2.suit;
            var dvalue2 = dcard2.value;
            dvalues.Add(dvalue);
            dtvalues.Add(dvalue);
            dtvalues.Add(dvalue2);
            dduplicate.TryAdd(new KeyValuePair<string, int>(suit2, value2), 1);
            var cards = new EmbedBuilder()
                .WithColor(Color.LightOrange)
                .WithTitle("Blackjack!")
                .AddField("Your cards:", $"({card.card} of {suit}), ({card2.card} of {suit2})")
                .AddField("Card total:", $"{values.Sum()}")
                .AddField("Dealers visible cards:", $"({dcard.card} of {dsuit}), (hidden)")
                .AddField("Dealers visible total:", $"{dvalues.Sum()}")
                .AddField("Available choices:", "Hit :punch, Stay :hand_splayed:, Double :v: ")
                .Build();
            return cards;
        }

        [RequireContext(ContextType.Guild)]
        [Command("blackjack", RunMode = RunMode.Async)]
        [Summary("Starts a round of blackjack usage: gamble <amount of mangos>")]
        public async Task Gamble(int bet)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id))
            {
                dbo.UserCurrency.TryAdd(Context.User.Id, 0);
            }
            userData.TryAdd(Context.User.Id, _deck);
            userDData.TryAdd(Context.User.Id, _deck);
            var mangos = dbo.UserCurrency[Context.User.Id];
            if (bet > 0 && mangos < bet) { await ReplyAsync($"You do not have enough mangos to bet that! You have {mangos} Mangos!"); return; }
            if (bet <= 0) { await ReplyAsync($"Bet must be greater than 0! You have {mangos} Mangos!"); return; }
            var embed = MyEmbed(Context.User.Id);
            await ReplyAsync($"You started a game of blackjack! You have bet {bet} Mangos!");
            var msg = await ReplyAsync(string.Empty, false, embed);
            var next = await ReplyAsync("Do you: hit:, stay, or double? You have 10 seconds to choose!", false, null);
            var reply = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
            if (reply.Content.Equals("hit", StringComparison.OrdinalIgnoreCase)) { await Hit(embed, msg, bet, next); }
            if (reply.Content.Equals("stay", StringComparison.OrdinalIgnoreCase)) { await Stay(embed, next, bet); }
            if (reply.Content.Equals("double", StringComparison.OrdinalIgnoreCase)) { await Double(next, bet, embed, msg); }
        }

        public async Task Hit(Embed embed, IUserMessage next, int bet, IUserMessage msg)
        {
            var deck = userData[Context.User.Id];
            var ddeck = userDData[Context.User.Id];
            if (dvalues.Sum() < 16) { await DealerHit(embed, msg, bet, next); }
            await next.DeleteAsync();
            var card = deck.Dequeue();
            var suit = card.suit;
            var value = card.value;
            var field = embed.Fields.FirstOrDefault(x => x.Name.Contains("cards"));
            var dfield = embed.Fields.FirstOrDefault(x => x.Name.Contains("Dealers"));
            if (duplicate.ContainsKey(new KeyValuePair<string, int>(suit, value))) { await Hit(embed, msg, bet, next); }
            values.Add(value);
            duplicate.TryAdd(new KeyValuePair<string, int>(suit, value), duplicate.Keys.Count + 1);
            var newembed = new EmbedBuilder()
                .WithColor(Color.LightOrange)
                .WithTitle("Blackjack!")
                .AddField("Your cards:", $"{field.Value}, ({value} of {suit})")
                .AddField("Card total:", $"{values.Sum()}")
                .AddField("Dealers visible cards:", $"{dfield.Value}")
                .AddField("Dealers visible total:", $"{dvalues.Sum()}")
                .AddField("Available choices:", "Hit, Stay, Double")
                .Build();
            await msg.ModifyAsync(x => x.Embed = newembed);
            if (values.Sum() > 21) { await WinLose(values.Sum(), dtvalues.Sum(), bet); return; }
            var nextmsg = await ReplyAsync("Do you: hit, stay, or double? You have 10 seconds to choose!", false, null);
            var reply = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
            if (reply.Content.Equals("hit", StringComparison.OrdinalIgnoreCase)) { await Hit(newembed, msg, bet, nextmsg); }
            if (reply.Content.Equals("stay", StringComparison.OrdinalIgnoreCase)) { await Stay(newembed, nextmsg, bet); }
            if (reply.Content.Equals("double", StringComparison.OrdinalIgnoreCase)) { await Double(nextmsg, bet, embed, msg); }
        }

        public async Task DealerHit(Embed embed, IUserMessage next, int bet, IUserMessage msg = null)
        {
            var deck = userData[Context.User.Id];
            var ddeck = userDData[Context.User.Id];
            var field = embed.Fields.FirstOrDefault(x => x.Name.Contains("cards"));
            var dfield = embed.Fields.FirstOrDefault(x => x.Name.Contains("Dealers"));
            if (dvalues.Sum() < 16)
            {
                var card = ddeck.Dequeue(); dvalues.Add(card.value); dtvalues.Add(card.value); var type = card.card; var suit = card.suit;
                var newembed = new EmbedBuilder()
                .WithColor(Color.LightOrange)
                .WithTitle("Blackjack!")
                .AddField("Your cards:", $"{field.Value}")
                .AddField("Card total:", $"{values.Sum()}")
                .AddField("Dealers visible cards:", $"{dfield.Value} ({type} of {suit})")
                .AddField("Dealers visible total:", $"{dvalues.Sum()}")
                .AddField("Available choices:", "Hit, Stay, Double")
                .Build();
                await DealerHit(newembed, msg, bet, next);
            }
            await Hit(embed, msg, bet, next);
        }

        public async Task Stay(Embed embed, IUserMessage next, int bet)
        {
            if (dvalues.Sum() < 16) { await DealerHitStay(embed, next, bet); }
            await next.DeleteAsync();
            await ReplyAsync($"You have chosen to stay at a total of {values.Sum()}! Lets check what the dealer has!");
            var msg = await ReplyAsync($"Come on now.... don't be shy.");
            await Task.Delay(3000);
            await msg.ModifyAsync(x => x.Content = $"Aha! The dealer has a total of {dtvalues.Sum()}!");
            await WinLose(values.Sum(), dtvalues.Sum(), bet);
        }
        public async Task DealerHitStay(Embed embed, IUserMessage next, int bet)
        {
            var deck = userData[Context.User.Id];
            var ddeck = userDData[Context.User.Id];
            var field = embed.Fields.FirstOrDefault(x => x.Name.Contains("cards"));
            var dfield = embed.Fields.FirstOrDefault(x => x.Name.Contains("Dealers"));
            if (dvalues.Sum() < 16)
            {
                var card = ddeck.Dequeue(); dvalues.Add(card.value); dtvalues.Add(card.value); var type = card.card; var suit = card.suit;
                var newembed = new EmbedBuilder()
                .WithColor(Color.LightOrange)
                .WithTitle("Blackjack!")
                .AddField("Your cards:", $"{field.Value}")
                .AddField("Card total:", $"{values.Sum()}")
                .AddField("Dealers visible cards:", $"{dfield.Value} ({type} of {suit})")
                .AddField("Dealers visible total:", $"{dvalues.Sum()}")
                .AddField("Available choices:", "Hit, Stay, Double")
                .Build();
                await DealerHitStay(newembed, next, bet);
            }
            await WinLose(values.Sum(), dvalues.Sum(), bet);
        }


        public async Task Double(IUserMessage next, int bet, Embed embed, IUserMessage msg)
        {
            var deck = userData[Context.User.Id];
            var ddeck = userDData[Context.User.Id];
            await next.DeleteAsync();
            var card = deck.Dequeue();
            var suit = card.suit;
            var value = card.value;
            var field = embed.Fields.FirstOrDefault(x => x.Name.Contains("cards"));
            var dfield = embed.Fields.FirstOrDefault(x => x.Name.Contains("Dealers"));
            if (duplicate.ContainsKey(new KeyValuePair<string, int>(suit, value))) { await Double(next, bet, embed, msg); }
            values.Add(value);
            duplicate.TryAdd(new KeyValuePair<string, int>(suit, value), duplicate.Keys.Count + 1);
            await ReplyAsync($"Are you crazy?? You just doubled your bet {bet}! You have placed {bet * 2} on the line!");
            var newembed = new EmbedBuilder()
                .WithColor(Discord.Color.LightOrange)
                .WithTitle("Blackjack!")
                .AddField("Your cards:", $"{field.Value}, ({value} of {suit})")
                .AddField("Card total:", $"{values.Sum()}")
                .AddField("Dealers visible cards:", $"{dfield}")
                .AddField("Dealers visible total:", $"{dvalues.Sum()}")
                .Build();
            await msg.ModifyAsync(x => x.Embed = newembed);
            await WinLose(values.Sum(), dtvalues.Sum(), bet);
        }

        public async Task WinLose(int player, int dealer, int bet)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var userMangos = dbo.UserCurrency[Context.User.Id];
            int mangos = bet * _randomizer.Next(1, 4);
            bool? win = null;
            if (player == dealer)
            {
                await ReplyAsync("Push! You keep your Mangos!");
                await MangosUpdate(win, userMangos);
            }
            else if (player > 21 && dealer <= 21)
            {
                await ReplyAsync($"Bust! You busted! You have lost {bet} Mangos!");
                win = false;
            }
            else if (dealer > 21 && player < 21)
            {
                await ReplyAsync($"You won! Dealer busted! You won {mangos} mangos!");
                win = true;

            }
            else if (player > 21 && dealer > 21)
            {
                await ReplyAsync("Push! You both busted!");
                return;
            }
            else if (player <= 21 && player > dealer)
            {
                await ReplyAsync($"You won! You have won {mangos} Mangos!");
                win = true;
            }
            else if (player < 21 && player < dealer)
            {
                await ReplyAsync($"You lost! You have lost {bet} Mangos!");
                win = false;
            }
            if (player == 21 && dealer < 21)
            {
                await ReplyAsync($"You won! Blackjack!!! You won {mangos * 2} Mangos!");
                win = true;
            }
            if (player == 21 && dealer == 21)
            {
                await ReplyAsync("Push! You keep your Mangos!");
            }
            await MangosUpdate(win, userMangos);
        }

        public async Task MangosUpdate(bool? win, int mangos)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var userMangos = dbo.UserCurrency[Context.User.Id];
            if ((win == true) && (userMangos == int.MaxValue)) { await ReplyAsync("You have the max amount of Mangos!"); }
            if (win == true)
            {
                userMangos = userMangos + mangos;
            }
            else if (win == false)
            {
                userMangos = userMangos - mangos;
            }
            dbo.UserCurrency[Context.User.Id] = userMangos;
            _db.StoreObject<GuildObjects>(dbo, Context.Guild.Id);
            await ReplyAsync($"You now have {userMangos} Mangos!");
        }
    }
}