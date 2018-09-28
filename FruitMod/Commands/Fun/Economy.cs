using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using static FruitMod.Economy.Store;

namespace FruitMod.Commands.Fun
{
    public class Economy : ModuleBase<FruitModContext>
    {
        private readonly DbService _db;
        private readonly DiscordSocketClient _client;

        public Economy(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        [Command("guild shop")]
        [Summary("Shows you the guild fruit shop")]
        public async Task ShowShop()
        {
            var items = new StringBuilder();
            items.AppendLine(string.Format("[{0,-18}]", "== AutoFarmers =="));
            foreach (var item in guildModifiers.Keys)
            {
                if (!shopPrices.ContainsKey(item)) continue;

                var newItem = item.ToString();

                items.AppendLine(string.Format("[{0,-18} {1, 3}% || Cost: ${2, -6}]", newItem.Replace("_", " "),
                    guildModifiers[item], shopPrices[item]));
            }

            items.Insert(items.ToString().LastIndexOf("[Farmers              2% || Cost: $500   ]"),
                "== Production Boosters ==\n");

            await ReplyAsync($"Purchasable guild items:\n{Format.Code(items.ToString(), "ini")}");
        }

        [Command("guild buy", RunMode = RunMode.Async)]
        [Summary("Uses the guild's fruit to buy a perk!")]
        public async Task ShopBuy(Shop modifier = Shop.Starter_Garden)
        {
            if (modifier is Shop.Starter_Garden)
            {
                await ReplyAsync("You must select a guild modifier! (This is case sensitive) please see the guild shop!");
                return;
            }
            var dbo = _db.GetById<GlobalCurrencyObject>("GCO");
            if (dbo.GuildCurrencyValue[Context.Guild.Id] < shopPrices[modifier])
            {
                await ReplyAsync("The guild does not have enough fruit to purchase this upgrade!");
                return;
            }
            dbo.GuildCurrencyValue[Context.Guild.Id] -= shopPrices[modifier];
            dbo.GuildModifiers[Context.Guild.Id].Add(modifier);
            _db.StoreObject(dbo, "GCO");
            await ReplyAsync($"Modifier {modifier} has been purchased! Remaining value: {dbo.GuildCurrencyValue[Context.Guild.Id]}");
        }

        [Command("guild value")]
        [Summary("Shows the value of this guild")]
        public async Task GuildValue()
        {
            var dbo = _db.GetById<GlobalCurrencyObject>("GCO");

            await ReplyAsync(
                $"This guild's global value: {dbo.GuildCurrencyValue[Context.Guild.Id]}\n The guild gains more fruit every 30min. The amount depends on the purchased modifiers.");
        }
    }
}