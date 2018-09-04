using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using QuickType;

namespace FruitMod.Commands.NSFWCommands
{
    [RequireNsfw]
    public class NSFWCommands : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient _http;
        private readonly Random _random;

        public NSFWCommands(Random random, HttpClient http)
        {
            _random = random;
            _http = http;
        }

        [Command("tits")]
        [Summary("Gets a pair of tits")]
        public async Task Tits()
        {
            var select = _random.Next(5000);
            var resultant = JArray.Parse(await _http.GetStringAsync($"http://api.oboobs.ru/boobs/{select}")).First;
            await ReplyAsync($"http://media.oboobs.ru/{resultant["preview"]}");
        }

        [Command("booty")]
        [Summary("Gets a pair of tits")]
        public async Task Booty()
        {
            var select = _random.Next(5000);
            var resultant = JArray.Parse(await _http.GetStringAsync($"http://api.obutts.ru/butts/{select}")).First;
            await ReplyAsync($"http://media.obutts.ru/{resultant["preview"]}");
        }

        [Command("rule34", RunMode = RunMode.Async)]
        [Summary("rule34s your term")]
        public async Task Rule34([Remainder] string term)
        {
            var json = await _http.GetStringAsync($"https://r34-json-api.herokuapp.com/posts?tags={term}&limit=10");
            var stuff = Welcome.FromJson(json);
            await ReplyAsync(stuff.FirstOrDefault()?.FileUrl);
        }
    }
}