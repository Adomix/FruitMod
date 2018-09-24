using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static FruitMod.Economy.Store;

namespace FruitMod.Economy
{
    [SetService]
    public class GuildFactoryTimer
    {
        private readonly Timer _timer;
        private readonly DbService _db;
        private readonly DiscordSocketClient _client;

        public GuildFactoryTimer(DiscordSocketClient client, DbService db)
        {
            _db = db;
            _client = client;

            _timer = new Timer(_ =>
            {
                var dbo = _db.GetById<GlobalCurrencyObject>("GCO");
                Shop[] AutoMods = { Shop.Moms_Garden, Shop.Small_Factory, Shop.Large_Factory, Shop.Starter_Garden };
                var newTotals = new Dictionary<ulong, int>();
                foreach (var id in dbo.AutomatedGuilds.Keys)
                {
                    int total = 0;
                    if (dbo.GuildModifiers[id].Intersect(AutoMods).Any())
                    {
                        foreach (var modifier in dbo.GuildModifiers[id])
                        {
                            total += guildModifiers[modifier];
                        }
                        total = dbo.AutomatedGuilds[id] * (total / 100);
                        if (total == 0)
                        {
                            total = 50;
                        }
                        newTotals.Add(id, total);
                        Console.WriteLine($"Total updated for {id}!");
                    }
                }

                foreach (var id in newTotals)
                {
                    dbo.GuildCurrencyValue[id.Key] += id.Value;
                }

                _db.StoreObject(dbo, "GCO");
            },
            null,
            TimeSpan.FromMinutes(30),  // 4) Time that message should fire after the timer is created
            TimeSpan.FromMinutes(30)); // 5) Time after which message should repeat (use `Timeout.Infinite` for no repeat)
        }

        public Task StartTimer()
        {
            _timer.Change(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
            Console.WriteLine("Guild factory timer initialized!");
            return Task.CompletedTask;
        }

    }
}