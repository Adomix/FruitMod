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
        private readonly PushBullet _pb;

        public StatisticsService(DiscordSocketClient client, DbService db, CommandHandlingService commands,
            LoggingService log, PushBullet pb)
        {
            _client = client;
            _db = db;
            _commands = commands;
            _log = log;
            _pb = pb;
        }

        public void LoadStats()
        {
            _client.Ready += GuildCount;
            _client.JoinedGuild += JGuild;
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
            var exception = new LogMessage(LogSeverity.Info, "Guild", $"Joined: {guild.Name} || Total: {_client.Guilds.Count}");
            await _log.Log(exception);
            await _pb.SendNotificationAsync($"Joined {guild.Name}!");
            await guild.Owner.SendMessageAsync("Wow! Thanks for adding my bot, my name is Mango. I would like to know how you found it! Please tell me by typing `@FruitMod#2261 reply message here` thank you!");
        }

        // _client.LeftGuild += LGuild;
        private async Task LGuild(SocketGuild guild)
        {
            var exception = new LogMessage(LogSeverity.Info, "Guild", $"Left: {guild.Name} || Total: {_client.Guilds.Count}");
            await _log.Log(exception);
            await _pb.SendNotificationAsync($"Left {guild.Name}!");
            _db.DeleteObject(guild.Id);
        }
    }
}