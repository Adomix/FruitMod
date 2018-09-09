﻿using System;
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
        private readonly DbService _db;
        private readonly CommandHandlingService _commands;
        private readonly RatelimitService _rls;
        private readonly LoggingService _log;

        public SortedDictionary<ulong, List<SocketUserMessage>> delmsgs { get; set; } = new SortedDictionary<ulong, List<SocketUserMessage>>();

        public GuildService(DiscordSocketClient client, DbService db, CommandHandlingService commands, RatelimitService rls,
            LoggingService log)
        {
            _client = client;
            _db = db;
            _commands = commands;
            _rls = rls;
            _log = log;
        }

        public void GuildServices()
        {
            _client.MessageDeleted += DeletedMessageLogging;
            _client.UserLeft += UserLeftLogging;
            _client.UserJoined += CheckMuted;
            _client.MessageReceived += RlMsg;
        }

        private Task RlMsg(SocketMessage msg)
        {
            Task.Run(async () =>
            {
                if (!(msg.Channel is SocketTextChannel channel)) return;
                if (_rls.rlb[channel.Id] == false) return;
                if (!(_rls.msgdict.ContainsKey((channel.Id, msg.Author.Id))))
                {
                    _rls.msgdict.Add((channel.Id, msg.Author.Id), DateTime.UtcNow);
                }
                else
                {
                    if (msg.CreatedAt.UtcDateTime > _rls.msgdict[(channel.Id, msg.Author.Id)].AddSeconds(_rls.time))
                    {
                        _rls.msgdict[(channel.Id, msg.Author.Id)] = DateTime.Now;
                        await msg.DeleteAsync();
                    }
                }
            });
            return Task.CompletedTask;
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

                if(delmsgs.Values.Count >= 50)
                {
                    delmsgs.OrderByDescending(x => x.Value);
                    delmsgs.RemoveNext(10);
                }
                if (!delmsgs.Keys.Contains((umsg.Channel as SocketTextChannel).Guild.Id))
                {
                    delmsgs.Add((msg.Channel as SocketTextChannel).Guild.Id, new List<SocketUserMessage>() { umsg });
                }
                else
                    delmsgs[(msg.Channel as SocketTextChannel).Guild.Id].Add(umsg);

                if (!dbo.Settings.DeleteSys) return Task.CompletedTask;

                if (umsg.HasAttachments())
                    attachment = msg.Attachments.FirstOrDefault().Url;
                else
                    attachment = null;

                if (umsg.Author.IsBot)
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
                    .AddField($"User's {umsg.Author} message has been deleted!", Format.Code($"[{umsg.Content}]\nAttachments {umsg.Attachments.Count()}:[{attachment ?? "No attachment"}]", "ini"))
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