using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;

namespace FruitMod.Services
{
    [SetService]
    public class StatisticsService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandlingService _commands;
        private readonly DbService _db;
        private readonly LoggingService _log;

        public StatisticsService(DiscordSocketClient client, DbService db, CommandHandlingService commands, LoggingService log)
        {
            _client = client;
            _db = db;
            _commands = commands;
            _log = log;
        }

        public void LoadStats()
        {
            _client.Ready += GuildCount;
            //_client.JoinedGuild += JGuild;
            _client.LeftGuild += LGuild;
        }

        // _client.Ready += GuildCount;
        private Task GuildCount()
        {
            var guildLog = new LogMessage(LogSeverity.Info, "GuildCount()",
                $"Guilds: {_client.Guilds.Count} Users: {_client.Guilds.Sum(x => x.MemberCount)}");
            return _log.Log(guildLog);
        }

        // _client.JoinedGuild += JGuild;
        private async Task JGuild(SocketGuild guild)
        {
            Console.WriteLine("Joined " + guild.Name + " " + guild.Id, Color.Green);
            Console.WriteLine("Guild count: " + _client.Guilds.Count, Color.Magenta);
            Console.WriteLine("Total users affected: " + _client.Guilds.Sum(x => x.MemberCount), Color.Magenta);
            await guild.Owner.SendMessageAsync($"Wow! Thanks for adding my bot, my name is Mango. I would like to know how you found it! Please tell me by typing `@FruitMod#2261 reply message here` thank you!");
        }

        // _client.LeftGuild += LGuild;
        private Task LGuild(SocketGuild guild)
        {
            Console.WriteLine("Left " + guild.Name, Color.DarkRed);
            Console.WriteLine("Guild count: " + _client.Guilds.Count, Color.Magenta);
            Console.WriteLine("Total users affected: " + _client.Guilds.Sum(x => x.MemberCount), Color.Magenta);
            _db.DeleteObject(guild.Id);
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}