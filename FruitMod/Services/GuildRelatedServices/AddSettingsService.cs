using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Objects;

namespace FruitMod.Services
{
    [SetService]
    public class AddSettingsService
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public AddSettingsService(DiscordSocketClient client, DbService db)
        {
            _client = client;
            _db = db;
        }

        public void AddSettings()
        {
            _client.GuildAvailable += AddSettings;
            _client.GuildAvailable += CheckInfoChannel;
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