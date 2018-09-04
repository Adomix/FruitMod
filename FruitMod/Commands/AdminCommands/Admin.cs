﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Preconditions;

namespace FruitMod.Commands
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        private readonly DbService _db;
        private readonly DiscordSocketClient _client;

        public Admin(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        [RequireUserPermission(GuildPermission.BanMembers, Group = "admin")]
        [Command("ban")]
        [Summary("Bans targeted user, usage: !admin ban <user> <reason(optional>")]
        public async Task Ban(IUser user, [Remainder] string reason = null)
        {
            if (reason == null) reason = "x";
            await user.SendMessageAsync($"You have been banned from {Context.Guild.Name} by {Context.User}! Reason: {reason}");
            await Context.Guild.AddBanAsync(user, 0, $"{reason}");
            await ReplyAsync($"User {user} has been banned for {reason} by {Context.User.Username}!");
        }

        [RequireAnyUserPermAttribute(GuildPermission.MuteMembers, GuildPermission.ManageRoles, GuildPermission.ManageMessages, Group = "admin")]
        [Command("mute", RunMode = RunMode.Async)]
        [Summary("Text mutes or unmutes a user!")]
        public async Task Mute(IUser user)
        {
            if (user is null) await ReplyAsync("User not found!");
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (dbo.Settings.MuteRole is null)
            {
                await ReplyAsync("You must set a \"mute\" role! (Role without permission to type!) Use: setmute");
                return;
            }

            var roleId = dbo.Settings.MuteRole.Value;
            if (((SocketGuildUser)user).Roles.Any(x => x.Id == roleId))
            {
                if (user != null)
                {
                    await ((SocketGuildUser)user)?.RemoveRoleAsync(Context.Guild.GetRole(roleId));
                    dbo.UserSettings.MutedUsers.Remove(user.Id);
                    _db.StoreObject(dbo, Context.Guild.Id);
                    await ReplyAsync($"User {user.Mention} has been unmuted!");
                }
            }
            else
            {
                dbo.UserSettings.MutedUsers.Add(user.Id);
                await ((SocketGuildUser)user).AddRoleAsync(Context.Guild.GetRole(roleId));
                _db.StoreObject(dbo, Context.Guild.Id);
                await ReplyAsync($"User {user.Mention} has been muted!");
            }
        }

        [RequireAnyUserPermAttribute(GuildPermission.MuteMembers, GuildPermission.ManageRoles, GuildPermission.ManageMessages, Group = "admin")]
        [Command("vmute")]
        [Summary("Mutes or unmutes the targeted user, usage: !admin mute <user> <reason(optional>")]
        public async Task VMute(IGuildUser user, [Remainder] string reason = null)
        {
            if (!user.IsMuted)
            {
                await user.ModifyAsync(x => x.Mute = true);
                await ReplyAsync($"User muted! Reason: {reason}");
            }
            else
            {
                if (user.IsMuted)
                {
                    await user.ModifyAsync(x => x.Mute = false);
                    await ReplyAsync($"User unmuted! Reason: {reason}");
                }
            }
        }

        [RequireAnyUserPermAttribute(GuildPermission.MuteMembers, GuildPermission.ManageRoles, GuildPermission.ManageMessages, Group = "admin")]
        [Command("vblock")]
        [Summary("Mutes & deafens or mutes & undeafens the targeted user, usage: !admin block <user> <reason(optional>")]
        public async Task VBlock(IGuildUser user, string reason)
        {
            if (!user.IsMuted)
            {
                await user.ModifyAsync(x => x.Mute = true);
                if (!user.IsDeafened || !user.IsSelfDeafened) await user.ModifyAsync(x => x.Deaf = true);
            }
            else
            {
                if (user.IsMuted)
                {
                    await user.ModifyAsync(x => x.Mute = false);
                    if (user.IsDeafened || user.IsSelfDeafened) await user.ModifyAsync(x => x.Deaf = false);
                }
            }
        }

        [RequireAnyUserPermAttribute(GuildPermission.Administrator, GuildPermission.ManageChannels, GuildPermission.ManageMessages, GuildPermission.ManageGuild, GuildPermission.KickMembers, GuildPermission.BanMembers, Group = "admin")]
        [Command("block", RunMode = RunMode.Async)]
        [Summary("Blocks or unblocks a user from using the bot!")]
        public async Task Block(IUser user)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var guildId = Context.Guild.Id;
            var existinglist = _db.GetById<GuildObjects>(guildId).UserSettings.BlockedUsers ?? new List<ulong>();

            if (existinglist.Contains(user.Id))
            {
                existinglist.Remove(user.Id);
                dbo.UserSettings.BlockedUsers = existinglist;
                _db.StoreObject(dbo, Context.Guild.Id);
                await ReplyAsync($"User {user} has been unblocked!");
                return;
            }
            else
            {
                existinglist.Add(user.Id);
                dbo.UserSettings.BlockedUsers = existinglist;
                _db.StoreObject(dbo, Context.Guild.Id);
                var blockedEmbed = new EmbedBuilder()
                    .WithTitle("User bot blocked!")
                    .AddField($"{user.Mention}", "Proecced at time (GMT-5): ", true)
                    .AddField(" has been deauthorized from FruitMod!", DateTime.Now, true)
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Color.Blue)
                    .Build();
                await ReplyAsync(string.Empty, false, blockedEmbed);
            }
        }

        [RequireAnyUserPerm(GuildPermission.ManageChannels, GuildPermission.ManageMessages, Group = "admin")]
        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clears X amount of messages, usage: !admin clear <# of messages>")]
        public async Task Clear(int msgs = 5)
        {
            if (msgs > 100)
            {
                await ReplyAsync("Can not go over 100 messages, that exceeds the discord rate limit!");
                return;
            }

            var messages = Context.Channel.GetCachedMessages(msgs);
            if (Context.Channel is ITextChannel channel) await channel.DeleteMessagesAsync(messages);
            var msg = await ReplyAsync($"Deleted {msgs} messages successfully!");
            await Task.Delay(5000);
            await msg.DeleteAsync();
        }

        [RequireAnyUserPerm(GuildPermission.ManageChannels, GuildPermission.ManageMessages, Group = "admin")]
        [Command("purge", RunMode = RunMode.Async)]
        [Summary("Purges a user, usage: !admin purge <user> <amount(default 500)>")]
        public async Task Purge(IUser user, int amount = 500)
        {
            var channel = Context.Channel as ITextChannel;
            var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
            var msgs = from message in messages
                       where message.Author.Id == user.Id &&
                             message.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14))
                       select message;
            if (channel != null) await channel.DeleteMessagesAsync(msgs);
            await ReplyAsync($"User @{user} has been purged!");
        }

        [RequireAnyUserPerm(GuildPermission.ManageChannels, GuildPermission.ManageMessages, Group = "admin")]
        [Command(".fm", RunMode = RunMode.Async)]
        [Summary("Removes fruitmod posts in this channel")]
        public async Task FmRemove()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (Context.Channel.Id == dbo.Settings.LogChannel) await ReplyAsync("You may not clear me in my log channel!");
            var msgs = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var delmsgs = from message in msgs
                          where message.Author.Id == Context.Client.CurrentUser.Id &&
                                message.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14))
                          select message;
            if (Context.Channel != null) await ((ITextChannel)Context.Channel).DeleteMessagesAsync(delmsgs);
        }

        [RequireAnyUserPermAttribute(GuildPermission.Administrator, GuildPermission.ManageChannels, GuildPermission.ManageMessages, GuildPermission.ManageGuild, GuildPermission.KickMembers, GuildPermission.BanMembers, Group = "admin")]
        [RequireOwner(Group = "admin")]
        [Command("perms")]
        [Summary("Lists the bot's perms")]
        public async Task Perms()
        {
            await ReplyAsync("My permissions!:");
            await ReplyAsync($"{string.Join("\n", Context.Guild.GetUser(_client.CurrentUser.Id).GuildPermissions.ToList())}");
        }
    }
}