using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Commands.AudioCommands;
using FruitMod.Database;
using SharpLink;

namespace FruitMod.Services
{
    [SetService]
    public class EventHandlingService
    {
        private readonly AudioService _audio;

        private readonly DiscordSocketClient _client;
        private readonly CommandHandlingService _commands;
        private readonly DbService _db;
        private readonly LoggingService _log;
        private readonly LavalinkManager _manager;
        private PushBullet _pb;

        public EventHandlingService(DiscordSocketClient client, LavalinkManager manager, DbService db,
            AudioService audio, CommandHandlingService commands, LoggingService log, PushBullet pb)
        {
            _client = client;
            _manager = manager;
            _db = db;
            _audio = audio;
            _commands = commands;
            _log = log;
            _pb = pb;
        }

        public void InstallCommandsAsync()
        {
            _client.Ready += TestWarning;
            _client.Ready += TestError;
            _client.Connected += SetGame;
            _client.Disconnected += Disconnected;
            _client.Connected += Connected;
        }

        private Task Disconnected(Exception exception)
        {
            Task.Run(async () =>
            {
                await _pb.SendNotificationAsync($"FruitMod disconnected at {DateTimeOffset.UtcNow.AddHours(-5):MM/dd : HH:mm}");
                return;
            });
            return Task.CompletedTask;
        }

        private Task Connected()
        {
            Task.Run(async () =>
            {
                await _pb.SendNotificationAsync($"FruitMod Connected at {DateTimeOffset.UtcNow.AddHours(-5):MM/dd : HH:mm}");
                return;
            });
            return Task.CompletedTask;
        }

        // _client.Connected += SetGame
        private async Task SetGame()
        {
            await _client.SetGameAsync("@Fruitmod#2261 help", null, ActivityType.Streaming);
        }

        // _client.Ready += TestWarning (Warning barely shows up in console, this is to check proper functionality)
        private Task TestWarning()
        {
            var message = new LogMessage(LogSeverity.Warning, "TestWarning()", "Test warning!");
            return _log.Log(message);
        }

        // _client.Ready += TestWarning (Warning barely shows up in console, this is to check proper functionality)
        private Task TestError()
        {
            var message = new LogMessage(LogSeverity.Error, "TestError()", "Test error!");
            return _log.Log(message);
        }
    }
}