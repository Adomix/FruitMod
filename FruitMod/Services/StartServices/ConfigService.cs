using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Interactive;
using Microsoft.Extensions.DependencyInjection;
using SharpLink;
using Console = Colorful.Console;

namespace FruitMod.Services
{
    public class ConfigService
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private HttpClient _http;
        private LoggingService _log;
        private LavalinkManager _manager;
        private Random _random;
        private IServiceCollection _services;

        public async Task LaunchAsync()
        {
            Console.ResetColor();
            var config = new DiscordSocketConfig
                {MessageCacheSize = 100, LogLevel = LogSeverity.Verbose, AlwaysDownloadUsers = false};
            var sconfig = new CommandServiceConfig
                {CaseSensitiveCommands = false, LogLevel = LogSeverity.Debug};
            _client = new DiscordSocketClient(config);
            _http = new HttpClient();
            _log = new LoggingService();
            _random = new Random();

            _manager = new LavalinkManager(_client, new LavalinkManagerConfig
            {
                RESTHost = "localhost",
                LogSeverity = LogSeverity.Verbose,
                RESTPort = 2333,
                WebSocketHost = "localhost",
                WebSocketPort = 82,
                Authorization = ConfigurationManager.AppSettings["lava"],
                TotalShards = 1 // Please set this to the total amount of shards your bot uses
            });

            _client.Log += Log;
            _manager.Log += LavalinkLog;

            _commands = new CommandService(sconfig);
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_http)
                .AddSingleton(_commands)
                .AddSingleton(_manager)
                .AddSingleton(_random)
                .AddSingleton<InteractiveService>();

            var service = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(y => y.GetCustomAttributes(typeof(SetServiceAttribute), true).Length > 0);
            foreach (var services in service)
                _services.AddSingleton(services);
            var builtService = _services.BuildServiceProvider();

            await builtService.GetService<LaunchService>().StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            return _log.Log(message);
        }

        private Task LavalinkLog(LogMessage message)
        {
            return _log.LavalinkLog(message);
        }
    }
}