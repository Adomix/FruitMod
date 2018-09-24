using Discord;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Objects;
using System.Threading.Tasks;
using static FruitMod.Economy.Economy;
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

        [Command("shop")]
        [Summary("Shows you the fruit shop")]
        public async Task ShowShop()
        {
            string items = "";
            string fixedItem = null;
            foreach (var item in shopModifiers.Keys)
            {
                var newItem = item.ToString();
                if (newItem.Contains("_"))
                {
                    fixedItem = newItem.Replace("_", " ");
                    items += $"[{fixedItem} + {shopModifiers[item]}% || Cost: ${shopPrices[item]}]\n";
                }
                else
                {
                    items += $"[{newItem} + {shopModifiers[item]}% || Cost: ${shopPrices[item]}]\n";
                }
            }
            await ReplyAsync($"Purchasable items:\n{Format.Code(items, "ini")}");
        }

        [Command("value")]
        [Summary("Shows the value of this guild")]
        public async Task GuildValue()
        {
            var dbo = _db.GetById<GlobalCurrencyObject>("GCO");
            var value = _db.GetById<GuildObjects>(Context.Guild.Id);
            int total = 0;
            foreach (var user in Context.Guild.Users)
            {
                if (value.UserStruct.ContainsKey(user.Id))
                {
                    foreach (var fruit in value.UserStruct[user.Id].Fruit)
                    {
                        total += fruit.Value * fruitValues[fruit.Key];
                    }
                }
            }
            dbo.GuildCurrencyValue[Context.Guild.Id] = total;
            _db.StoreObject(dbo, "GCO");
            await ReplyAsync($"This guild's global value: {dbo.GuildCurrencyValue[Context.Guild.Id]}");
        }
    }
}