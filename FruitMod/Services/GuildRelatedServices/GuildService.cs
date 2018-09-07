using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Extensions;
using System.Linq;
using System.Collections.Generic;

namespace FruitMod.Services
{
    [SetService]
    public class GuildService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandlingService _commands;
        private readonly DbService _db;
        private readonly LoggingService _log;

        public SortedDictionary<(int, ulong), SocketUserMessage> delmsgs { get; set; } = new SortedDictionary<(int, ulong), SocketUserMessage>();
        private static int setter = 0;

        public GuildService(DiscordSocketClient client, DbService db, CommandHandlingService commands,
            LoggingService log)
        {
            _client = client;
            _db = db;
            _commands = commands;
            _log = log;
        }

        public void GuildServices()
        {
            _client.MessageDeleted += DeletedMessageLogging;
            _client.UserLeft += UserLeftLogging;
            _client.UserJoined += CheckMuted;
        }

        // _client.MessageDeleted += DeletedMessageLogging;
        private async Task DeletedMessageLogging(Cacheable<IMessage, ulong> oldmsg, ISocketMessageChannel channel)
        {
            ITextChannel chan;
            if (channel is IGuildChannel) chan = channel as ITextChannel;
            else
                return;
            var dbo = _db.GetById<GuildObjects>(chan?.Guild.Id);
            await Delete(dbo, oldmsg, chan);
        }

        private Task Delete(GuildObjects dbo, Cacheable<IMessage, ulong> oldmsg, ITextChannel channel)
        {
            Task.Run(async () =>
            {
                string attachment;

                _db.StoreObject(dbo, channel.Guild.Id);

                var msg = await oldmsg.GetOrDownloadAsync();

                if (!(msg is SocketUserMessage umsg)) return Task.CompletedTask;

                if(delmsgs.Count > 100)
                {
                    delmsgs.OrderByDescending(x => x.Key.Item1);
                    delmsgs.RemoveNext(10);
                }
                setter++;
                delmsgs.Add((setter, msg.Author.Id), umsg);

                if (!dbo.Settings.DeleteSys) return Task.CompletedTask;

                if (umsg.HasAttachments())
                    attachment = msg.Attachments.FirstOrDefault().Url;
                else
                    attachment = null;

                if (umsg.Author.Username.Equals("FruitMod"))
                    return Task.CompletedTask;

                if (dbo.Settings.LogChannel == null)
                {
                    var x = await channel.Guild.GetOwnerAsync();
                    await x.SendMessageAsync("Please set up a log channel to use logging! prefix setlogs logchannel");
                }
                else
                {
                    var embed = new EmbedBuilder()
                    .WithTitle("A message has been deleted!")
                    .AddField($"User's {umsg.Author} message has been deleted!", Format.Code($"[{umsg.Content}]\nAttachment:[{attachment ?? "No attachment"}]", "ini"))
                    .AddField($"From channel:", $"```ini\n[{umsg.Channel}]\n```")
                    .WithFooter($"Deleted at: {DateTime.UtcNow.AddHours(-4): M/d/y h:mm:ss tt} EST")
                    .WithColor(Color.Red)
                    .Build();
                    if ((channel as SocketTextChannel)?.Guild.GetChannel(dbo.Settings.LogChannel.Value) is SocketTextChannel newChannel)
                        await newChannel.SendMessageAsync(string.Empty, false, embed);
                }
                return Task.CompletedTask;
            });
            return Task.CompletedTask;
        }

        // _client.UserLeft += UserLeftLogging;
        private Task UserLeftLogging(SocketGuildUser user)
        {
            var message = new LogMessage(LogSeverity.Critical, "UserLeftLogging",
                $"User {user.Username} has left guild {user.Guild}");
            Task.Run(() =>
            {
                if (_db.GetById<GuildObjects>(user.Guild.Id) == null) return Task.CompletedTask;
                var dbo = _db.GetById<GuildObjects>(user.Guild.Id);
                if (dbo.Settings.LeaveSys == false) return Task.CompletedTask;
                if (dbo.Settings.LogChannel != null)
                {
                    var logChannel = _db.GetById<GuildObjects>(user.Guild.Id).Settings.LogChannel;
                    if (logChannel != null)
                    {
                        var channel = user.Guild.GetChannel(logChannel.Value);
                        (channel as SocketTextChannel)?.SendMessageAsync($"User {user} has left!");
                    }
                }

                return _db.GetById<GuildObjects>(user.Guild.Id).Settings.LeaveSys == false
                    ? Task.CompletedTask
                    : _log.Log(message);
            });
            return Task.CompletedTask;
        }

        // _client.Joined += CheckMuted;
        private Task CheckMuted(SocketGuildUser user)
        {
            var dbo = _db.GetById<GuildObjects>(user.Guild.Id);
            if (dbo.UserSettings.MutedUsers.Contains(user.Id))
                if (dbo.Settings.MuteRole != null)
                    user.AddRoleAsync(user.Guild.GetRole(dbo.Settings.MuteRole.Value));
            return Task.CompletedTask;
        }

    }
}