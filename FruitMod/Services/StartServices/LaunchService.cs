using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace FruitMod.Services
{
    [SetService]
    public class LaunchService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly PushBullet _pb;

        public LaunchService(DiscordSocketClient client, CommandService commands, IServiceProvider services, PushBullet pb)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _pb = pb;
        }

        public IEnumerable<string> Namespaces { get; set; }

        public async Task StartAsync()
        {
            await Login();
            _services.GetService<AddSettingsService>().AddSettings();
            _services.GetService<StatisticsService>().LoadStats();
            await CommandLoader();
            _services.GetService<CommandHandlingService>().CommandHandler();
            _services.GetService<EventHandlingService>().InstallCommandsAsync();
            GetNamespaces();
            _services.GetService<GuildService>().GuildServices();
            _services.GetService<SharplinkService>().AudioInitialization();
            NotifyLaunch();
        }


        private async Task Login()
        {
            await _client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["token"]);
            await _client.StartAsync();
        }

        private async Task CommandLoader()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private void GetNamespaces()
        {
            var namespaces = Assembly.GetEntryAssembly().GetTypes().Select(x => x.Namespace).Distinct().Skip(1);
            Namespaces = namespaces;
        }

        private void NotifyLaunch()
        {
            _pb.SendNotification($"FruitMod started at {DateTimeOffset.UtcNow.AddHours(-5)}");
        }
    }
}