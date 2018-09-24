using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;
using System.Text;
using System.Threading.Tasks;
using static FruitMod.Economy.Store;

namespace FruitMod.Commands.Fun
{
    public class Economy : ModuleBase<FruitModContext>
    {
        private readonly DbService _db;

        public Economy(DbService db)
        {
            _db = db;
        }

        [Command("guild shop")]
        [Summary("Shows you the guild fruit shop")]
        public async Task ShowShop()
        {
            var items = new StringBuilder();
            items.AppendLine(string.Format("[0,-18]", "== AutoFarmers =="));
            foreach (var item in guildModifiers.Keys)
            {
                if (!shopPrices.ContainsKey(item)) continue;

                var newItem = item.ToString();

                items.AppendLine(string.Format("[{0,-18} {1, 3}% || Cost: ${2, -6}]", newItem.Replace("_", " "), guildModifiers[item], shopPrices[item]));
            }

            items.Insert(items.ToString().LastIndexOf("[Farmers              2% || Cost: $500   ]") , "== Production Boosters ==\n");

            await ReplyAsync($"Purchasable items:\n{Format.Code(items.ToString(), "ini")}");
        }

        [Command("value")]
        [Summary("Shows the value of this guild")]
        public async Task GuildValue()
        {
            var dbo = _db.GetById<GlobalCurrencyObject>("GCO");

            await ReplyAsync($"This guild's global value: {dbo.GuildCurrencyValue[Context.Guild.Id]}\n The guild gains more fruit every 30min. The amount depends on the purchased modifiers.");
        }
    }
}