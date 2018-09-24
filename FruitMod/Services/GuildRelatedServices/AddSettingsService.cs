using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Economy;
using FruitMod.Objects;

namespace FruitMod.Services
{
    [SetService]
    public class AddSettingsService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly GuildFactoryTimer _timer;

        public AddSettingsService(DiscordSocketClient client, DbService db, GuildFactoryTimer timer)
        {
            _client = client;
            _db = db;
            _timer = timer;
        }

        public void AddSettings()
        {
            _client.GuildAvailable += AddSettings;
            _client.GuildAvailable += CheckInfoChannel;
            _client.Ready += GlobalGuildCurrency;
            _client.Ready += LaunchTimer;
        }

        private Task LaunchTimer()
        {
            return _timer.StartTimer();
        }

        private Task GlobalGuildCurrency()
        {
            Task.Run(() =>
            {
                var guilds = _client.Guilds;
                var dbo = _db.GetById<GlobalCurrencyObject>("GCO");

                if (dbo == null)
                {
                    var newGlobals = new Dictionary<ulong, int>();
                    var newModifiers = new Dictionary<ulong, List<Store.Shop>>();
                    foreach (var guild in guilds)
                    {
                        newGlobals.Add(guild.Id, 0);
                        newModifiers.Add(guild.Id, new List<Store.Shop>() { Store.Shop.Starter_Garden });
                    }
                    _db.StoreObject(new GlobalCurrencyObject { GuildCurrencyValue = newGlobals, AutomatedGuilds = newGlobals, GuildModifiers = newModifiers }, "GCO");
                }
                else
                {
                    foreach (var guild in guilds)
                    {
                        if (!dbo.GuildCurrencyValue.ContainsKey(guild.Id))
                        {
                            dbo.GuildCurrencyValue.Add(guild.Id, 0);
                        }
                    }
                    _db.StoreObject(dbo, "GCO");
                }
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        }

        // _client.GuildAvailable += CheckInfoChannel;
        private Task CheckInfoChannel(SocketGuild guild)
        {
            Task.Run(async () =>
            {
                var dbo = _db.GetById<GuildObjects>(guild.Id);
                if (dbo.Settings.InfoChannel == null) return;
                await guild.GetTextChannel(dbo.Settings.InfoChannel.Value).ModifyAsync(x =>
                    x.Topic =
                        $"FruitMod! Made by: Maͥnͣgͫo#4298 || Current Prefix(es): {string.Join(", ", dbo.Settings.Prefixes)} || More Help: https://discord.gg/NVjPVFX");
            });
            return Task.CompletedTask;
        }

        // _client.GuildAvailable += AddSettings;
        public Task AddSettings(SocketGuild guild)
        {
            Task.Run(() =>
            {
                try
                {
                    var dbo = _db.GetById<GuildObjects>(guild.Id);
                    if (dbo == null)
                        _db.StoreObject(new GuildObjects(), guild.Id);
                    else
                        _db.StoreObject(dbo, guild.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            return Task.CompletedTask;
        }
    }
}