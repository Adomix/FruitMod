using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Objects;

namespace FruitMod.Services
{
    [SetService]
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly DbService _db;
        private readonly LoggingService _log;
        private readonly IServiceProvider _services;

        public CommandHandlingService(DiscordSocketClient client, DbService db, CommandService commands,
            IServiceProvider services, LoggingService log)
        {
            _client = client;
            _db = db;
            _commands = commands;
            _services = services;
            _log = log;
        }

        public void CommandHandler()
        {
            _client.MessageReceived += HandleCommandAsync;
        }

        public Task HandleCommandAsync(SocketMessage messageParam)
        {
            _ = Task.Run(async () =>
            {
                if (!(messageParam is SocketUserMessage message) ||
                    messageParam.Author.IsBot && messageParam.Author.Id != _client.CurrentUser.Id) return;
                var guild = (message.Channel as SocketTextChannel)?.Guild;
                if (guild != null)
                {
                    var db = _db.GetById<GuildObjects>(guild.Id);
                    if (db.UserSettings.BlockedUsers.Contains(message.Author.Id)) return;

                    var prefixes = db.Settings.Prefixes;

                    var argPos = 0;
                    if (!(prefixes.Any(x => message.HasStringPrefix(x, ref argPos)) ||
                          message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
                    var context = new SocketCommandContext(_client, message);
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    var logMessage = new LogMessage(LogSeverity.Error, "CommandHandler",
                        $"{result.ErrorReason}: => \"{messageParam.Content}\"");
                    switch (result.Error)
                    {
                        case CommandError.UnmetPrecondition:
                            await context.Channel.SendMessageAsync("You do not have permission to use this command!");
                            break;
                        case CommandError.BadArgCount:
                            await context.Channel.SendMessageAsync(
                                "You did not supply the correct amount of arguments for this command. See prefix info <command>");
                            break;
                        case CommandError.ObjectNotFound:
                            await context.Channel.SendMessageAsync("Target not found!");
                            break;
                        default:
                            Console.WriteLine($"Default result error!: {result}");
                            break;
                    }

                    if (!result.IsSuccess)
                        if (result.ErrorReason.Contains("Unknown"))
                            await _log.Log(logMessage);
                }
            });
            return Task.CompletedTask;
        }
    }
}