using System;
using System.Collections.Concurrent;
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
    [RequireMods(Group = "Moderation")]
    [RequireGuildOwner(Group = "Moderation")]
    [RequireAnyUserPerm(GuildPermission.ManageRoles, GuildPermission.ManageGuild, Group = "Moderation")]
    [RequireOwner(Group = "Moderatinon")]
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public Moderation(DbService db, DiscordSocketClient client)
        {
            _db = db;
            _client = client;
        }

        [Command("kick")]
        [Summary("Kicks targeted user, usage: kick <user> <reason(optional)>")]
        public async Task Kick(IUser user, [Remainder] string reason = "x")
        {
            await user.SendMessageAsync($"You have been kicked from {Context.Guild.Name} by {Context.User}! Reason: {reason}");
            await Context.Guild.AddBanAsync(user, 0, $"{reason}");
            await Context.Guild.RemoveBanAsync(user);
        }

        [Command("ban")]
        [Summary("Bans targeted user, usage: ban <user> <length>(optional, default is perm) <reason(optional)>")]
        public async Task Ban(IUser user, int time = 0, [Remainder] string reason = "x")
        {
            await user.SendMessageAsync($"You have been banned from {Context.Guild.Name} by {Context.User}! Reason: {reason}");
            await Context.Guild.AddBanAsync(user.Id, time, $"{reason}");
        }

        [Command("bans")]
        [Summary("Shows the users banned")]
        public async Task Bans()
        {
            var bans = await Context.Guild.GetBansAsync();
            await ReplyAsync(string.Join("\n", bans));
        }

        [Command("mute", RunMode = RunMode.Async)]
        [Summary("Text mutes or unmutes a user!")]
        public async Task Mute([Remainder] SocketGuildUser user)
        {
            if (user is null)
            {
                await ReplyAsync("User not found!");
                return;
            }
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (dbo.Settings.MuteRole is null)
            {
                await ReplyAsync("You must set a \"mute\" role! (Role without permission to type!) Use: setmute");
                return;
            }

            var roleId = dbo.Settings.MuteRole.Value;
            if (user.Roles.Any(x => x.Id == roleId))
            {
                await user.RemoveRoleAsync(Context.Guild.GetRole(roleId));
                dbo.UserSettings.MutedUsers.Remove(user.Id);
                _db.StoreObject(dbo, Context.Guild.Id);
                await ReplyAsync($"User {user.Mention} has been unmuted!");
            }
            else
            {
                dbo.UserSettings.MutedUsers.Add(user.Id);
                await ((SocketGuildUser)user).AddRoleAsync(Context.Guild.GetRole(roleId));
                _db.StoreObject(dbo, Context.Guild.Id);
                await ReplyAsync($"User {user.Mention} has been muted!");
            }
        }

        [Command("vmute")]
        [Summary("Mutes or unmutes the targeted user, usage: !admin mute <user> <reason(optional>")]
        public async Task VMute(IGuildUser user, [Remainder] string reason = "x")
        {
            if (user is null) return;
            await user.ModifyAsync(x => x.Mute = !(bool)x.Mute);
            await ReplyAsync($"User {(user.IsMuted ? "muted" : "unmuted")}! Reason: {reason}");
        }

        [Command("vblock")]
        [Summary("Mutes & deafens or mutes & undeafens the targeted user, usage: !admin block <user> <reason(optional>")]
        public async Task VBlock(IGuildUser user, [Remainder] string reason = "x")
        {
            await user.ModifyAsync((x) =>
            {
                x.Mute = !(bool)x.Mute;
                x.Deaf = x.Mute;
            });
        }

        [Command("block", RunMode = RunMode.Async)]
        [Summary("Blocks or unblocks a user from using the bot!")]
        public async Task Block([Remainder] IUser user)
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
            await channel?.DeleteMessagesAsync(msgs);
            await ReplyAsync($"User @{user} has been purged!");
        }

        [Command(".fm", RunMode = RunMode.Async)]
        [Summary("Removes fruitmod posts in this channel")]
        public async Task FmRemove()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (Context.Channel.Id == dbo.Settings.LogChannel)
                await ReplyAsync("You may not clear me in my log channel!");
            var msgs = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var delmsgs = from message in msgs
                          where message.Author.Id == Context.Client.CurrentUser.Id &&
                              message.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14))
                          select message;
            (Context.Channel as ITextChannel)?.DeleteMessagesAsync(delmsgs);
        }

        [Command("mods")]
        [Summary("returns all the moderator roles")]
        public async Task Mods()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            await ReplyAsync(string.Join("\n", dbo.Settings.ModRoles.Select(x => x.Name)));
        }

        [Command("mangos give")]
        [Summary("gives x of mangos. Usage: agive 10 user")]
        public async Task AGive(int amount, [Remainder] IUser user)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(user.Id)) dbo.UserCurrency.TryAdd(user.Id, 0);
            var userGive = dbo.UserCurrency[user.Id];

            if (user is null)
            {
                await ReplyAsync("You must specify a user!");
                return;
            }

            if (amount <= 0)
            {
                await ReplyAsync("The amount must be greater than zero!");
                return;
            }

            if (amount >= int.MaxValue)
            {
                await ReplyAsync("No overflowing! Will set the user to the max!");
                amount = int.MaxValue - userGive;
            }
            if (userGive + amount == int.MaxValue)
                await ReplyAsync($"User is already at the max amount of Mangos! {int.MaxValue}");

            userGive += amount;
            dbo.UserCurrency[user.Id] = userGive;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have successfully given {user} {amount} Mangos!");
        }

        [Command("mangos give", RunMode = RunMode.Async)]
        [Summary("gives x of mangos. Usage: agive 10 @everyone")]
        public async Task AGive(int amount, [Remainder] IRole role)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (role is null)
            {
                await ReplyAsync("You must specify a role!");
                return;
            }

            if (amount <= 0)
            {
                await ReplyAsync("The amount must be greater than zero!");
                return;
            }

            foreach (IUser user in (role as SocketRole).Members)
            {
                if (!dbo.UserCurrency.ContainsKey(user.Id)) dbo.UserCurrency.TryAdd(user.Id, 0);

                if (amount >= int.MaxValue) amount = int.MaxValue - dbo.UserCurrency[user.Id];

                if (dbo.UserCurrency[user.Id] >= int.MaxValue) dbo.UserCurrency[user.Id] = int.MaxValue;

                dbo.UserCurrency[user.Id] += amount;
            }

            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"You have successfully given `{role.Name}` {amount} Mangos!");
        }

        [Command("mangos del", RunMode = RunMode.Async)]
        [Summary("Resets everyones mangos")]
        public async Task MangoR()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var currency = new ConcurrentDictionary<ulong, int>();
            foreach (var id in dbo.UserCurrency.Keys) currency.TryAdd(id, 0);

            dbo.UserCurrency = currency;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("All the Mangos have been eaten!");
        }

        [Command("mangos del", RunMode = RunMode.Async)]
        [Summary("Resets everyones mangos. Usage: mangor user")]
        public async Task MangoR([Remainder] IUser user)
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            if (!dbo.UserCurrency.ContainsKey(Context.User.Id)) dbo.UserCurrency.TryAdd(Context.User.Id, 0);

            dbo.UserCurrency[user.Id] = 0;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync($"User's {user} Mangos have been eaten!");
        }

        [Command("mangos delb", RunMode = RunMode.Async)]
        [Summary("Removes all bots added during mango admin operations")]
        public async Task Mangorb()
        {
            var dbo = _db.GetById<GuildObjects>(Context.Guild.Id);
            var y = new ConcurrentDictionary<ulong, int>();
            foreach (var x in dbo.UserCurrency)
            {
                var user = Context.Guild.GetUser(x.Key);
                if (!user.IsBot) y.TryAdd(x.Key, x.Value);
            }

            dbo.UserCurrency = y;
            _db.StoreObject(dbo, Context.Guild.Id);
            await ReplyAsync("All bots removed!");
        }
    }
}