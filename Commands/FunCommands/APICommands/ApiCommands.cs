using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace FruitMod.Commands.FunCommands.ApiCommands
{
    public class ApiCommands : ModuleBase<SocketCommandContext>
    {
        private readonly HttpClient _http;
        private readonly Random _randomizer = new Random();

        public ApiCommands(HttpClient http)
        {
            _http = http;
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
                JObject.Parse(await _http.GetStringAsync("http://api.urbandictionary.com/v0/define?term={word}"));
            await ReplyAsync(jResponse["list"][1]["definition"].ToString());
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
            var colorn = _randomizer.Next(color.Count + 1);
            var cpick = color[colorn];
            var response = await _http.GetStringAsync(
                $"https://img4me.p.mashape.com/?bcolor=%23{cpick}&fcolor=000000&font=trebuchet&size=12&text={text}&type=png");
            await ReplyAsync(response);
        }
    }
}