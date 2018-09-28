using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Extensions;
using FruitMod.Objects;
using FruitMod.Services;
using Humanizer;

namespace FruitMod.Commands
{
    public class General : ModuleBase<FruitModContext>
    {
        private static readonly Dictionary<ulong, DateTime> feedback = new Dictionary<ulong, DateTime>();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmd;
        private readonly DbService _db;
        private readonly GuildService _guildService;

        public General(DiscordSocketClient client, GuildService gs, CommandService cmd, DbService db)
        {
            _client = client;
            _guildService = gs;
            _cmd = cmd;
            _db = db;
        }

        [Command("info")]
        [Summary("Displays the bot's information")]
        public async Task Info()
        {
            var botInfo = await Context.Client.GetApplicationInfoAsync();
            var time = DateTime.Now - Process.GetCurrentProcess().StartTime;
            await ReplyAsync(
                $"About me: {Format.Code($"Name: [{botInfo.Name}] Id: [{botInfo.Id}]\nOwner: [{botInfo.Owner}] Status: [{Context.Client.Status}]\nUptime: [{time.Humanize()}] Connection: [{Context.Client.ConnectionState}]\nModules: [{_cmd.Modules.Count()}] Commands: [{_cmd.Commands.Count()}]\nSource: [https://github.com/Adomix/FruitMod]", "ini")}");
        }

        [Command("discord")]
        [Summary("provides the dev's discord")]
        public async Task DiscordInv()
        {
            await ReplyAsync(@"My discord is: Maͥnͣgͫo#4298");
        }

        [Command("modules", RunMode = RunMode.Async)]
        [Summary("lists all the modules")]
        public async Task Modules()
        {
            var modules = _cmd.Modules.OrderBy(x => x.Name).Select(x => x.Name).Aggregate((x, y) => $"{x}, {y}");
            await ReplyAsync($"Modules: {modules}");
        }

        [Command("ping")]
        [Summary("displays the bot's ping")]
        public async Task Ping()
        {
            var sw = new Stopwatch();
            sw.Start();
            var msg = await ReplyAsync("Pinging..");
            sw.Stop();
            await msg.ModifyAsync(x =>
                x.Content = $":clap: Ping: {sw.ElapsedMilliseconds}ms || :handshake: API: {_client.Latency}ms");
        }

        [Command("uptime")]
        [Summary("Gives the uptime of the bot")]
        public async Task Uptime()
        {
            var time = (DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize();
            await ReplyAsync($"The bot has been online for: {time}");
        }

        [Command("votekick", RunMode = RunMode.Async)]
        [Summary("Vote kicks a member, Usage: votekick <user> <reason(optional)>")]
        public async Task VoteKick(IUser user, [Remainder] string reason = null)
        {
            if (_db.GetById<GuildObjects>(Context.Guild.Id) is null)
            {
                await ReplyAsync("Creating database! Please try again!");
                return;
            }

            if (!_db.GetById<GuildObjects>(Context.Guild.Id).Settings.VoteSys)
            {
                await ReplyAsync("An administrator needs to turn this feature on!");
            }
            else
            {
                await ReplyAsync(
                    $"RIP! {Context.User.Username} has initiated a votekick against {user.Username} for {reason}! You have 60 seconds to vote! Vote opens in 2 seconds!");
                var msg = await ReplyAsync("How to vote: Click the check for yes, the X for no!");
                await msg.AddReactionsAsync(new[] {new Emoji("✅"), new Emoji("❌")});
                await Task.Delay(TimeSpan.FromMinutes(1));
                if (!(await Context.Channel.GetMessageAsync(msg.Id) is IUserMessage message)) return;

                await message.DeleteAsync();
                var check = message.Reactions.GetValueOrDefault(message.Reactions.Keys.Single(x => x.Name == "✅"))
                    .ReactionCount;
                var xemj = message.Reactions.GetValueOrDefault(message.Reactions.Keys.Single(x => x.Name == "❌"))
                    .ReactionCount;

                if (check > xemj)
                {
                    await ReplyAsync($"{user}, the tribe has spoken.");
                    await user.SendMessageAsync(
                        $"Kick details: Voted off by {Context.User.Username} for the reason {reason}");
                    await Context.Guild.GetUser(user.Id)?.KickAsync();
                }
                else if (check < xemj)
                {
                    await ReplyAsync($"{user} tonight, you are not being voted off the island.");
                }
                else if (check == xemj)
                {
                    await ReplyAsync($"{user}, the tribe is inconclusive, enjoy your stay.");
                }
            }
        }

        [Command("userinfo", RunMode = RunMode.Async)]
        [Summary("Gathers info on a user, Usage: userinfo <user>")]
        public async Task UserInfo([Remainder] IUser user)
        {
            if (!(user is SocketGuildUser suser)) return;

            var guildPerm = suser.GuildPermissions.ToList();
            var perms = guildPerm.Contains(GuildPermission.Administrator)
                ? "All permissions"
                : guildPerm.Count > 5
                    ? $"`{string.Join(", ", guildPerm)}`"
                    : string.Join(", ", guildPerm);

            var roles = suser.Roles.Count > 5
                ? $"`{string.Join(", ", suser.Roles.OrderBy(x => x.Position))}`"
                : string.Join(", ", suser.Roles.OrderBy(x => x.Position));
            var role = suser.Roles.OrderBy(x => x.Position).FirstOrDefault(x => x.Color != Color.Default);
            var color = role?.Color ?? Color.DarkPurple;

            var infoembed = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"User: {suser.Username}")
                .WithThumbnailUrl(suser.GetAvatarUrl() ?? suser.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .AddField("Nickname:", suser.Nickname ?? "No nickname", true)
                .AddField("ID:", suser.Id, true)
                .AddField("Discriminator:", suser.Discriminator, true)
                .AddField("Bot:", suser.IsBot, true)
                .AddField("Created:", suser.CreatedAt, true)
                .AddField("Joined:", suser.JoinedAt.Value.Date, true)
                .AddField("Highest Role:", suser.Roles.LastOrDefault(), true)
                .AddField("User Hierarchy:",
                    $"{(suser.Hierarchy == int.MaxValue ? "Guild Owner" : $"{suser.Hierarchy}")}", true)
                .AddField("All Roles:", roles)
                .AddField("Permissions:", perms)
                .AddField("Playing:", suser.Activity?.Name ?? "Not currently playing anything")
                .Build();
            await Context.Channel.SendMessageAsync(embed: infoembed);
        }

        [Command("avatar")]
        [Summary("Shows the users avatar. Usage: avatar <user>(optional)")]
        public async Task Avatar([Remainder] IUser user = null)
        {
            if (user is null) user = Context.User;

            if (!(user is SocketGuildUser suser)) return;

            var role = suser.Roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color != Color.Default);
            var color = role?.Color ?? Color.DarkPurple;

            var embed = new EmbedBuilder()
                .WithColor(color)
                .WithImageUrl(suser.GetAvatarUrl(size: 1024) ?? suser.GetDefaultAvatarUrl())
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("feedback")]
        [Summary("Gives the bot owner feedback")]
        public async Task Feedback([Remainder] string message)
        {
            if (feedback.ContainsKey(Context.User.Id) && feedback[Context.User.Id].Date == DateTime.Now.Date)
            {
                await ReplyAsync("You have already sent some feedback today!");
                return;
            }

            feedback.Add(Context.User.Id, DateTime.Now);
            await _client.GetUser(386969677143736330).SendMessageAsync($"Feedback from {Context.User} : {message}");
            await ReplyAsync("Thank you! Your feedback has been sent to the bot owner!");
        }

        [Command("blocklist", RunMode = RunMode.Async)]
        [Summary("Displays current guild block list")]
        public async Task ViewBList()
        {
            var blocklist = _db.GetById<GuildObjects>(Context.Guild.Id).UserSettings.BlockedUsers;
            if (blocklist is null)
            {
                await ReplyAsync("Nobody is blocked in this guild!");
                return;
            }

            var blockedembed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Block List")
                .AddField("Blocked IDs: ", string.Join(", ", blocklist))
                .WithCurrentTimestamp()
                .Build();
            await Context.Channel.SendMessageAsync(embed: blockedembed);
        }

        [Command("perms")]
        [Summary("Lists the bot's perms")]
        public async Task Perms()
        {
            await ReplyAsync(
                $"My permissions!:\n{string.Join("\n", Context.Guild.GetUser(_client.CurrentUser.Id).GuildPermissions.ToList())}");
        }

        [Command("snipe")]
        [Summary("snipes the last deleted mention")]
        public async Task Snipe()
        {
            var message = _guildService.delmsgs[Context.Guild.Id].LastOrDefault(x => x.MentionedUsers.Count > 0);
            if (message != null)
            {
                if (!(message.Author is SocketGuildUser author)) return;

                var role = author.Roles.OrderBy(x => x.Position).FirstOrDefault(x => x.Color != Color.Default);
                var color = role?.Color ?? Color.DarkPurple;

                var embed = new EmbedBuilder()
                    .WithCurrentTimestamp()
                    .WithColor(color)
                    .WithAuthor(author)
                    .WithTitle("Sniped!")
                    .AddField($"Mentions: ({message.MentionedUsers.Count})", string.Join(", ", message.MentionedUsers))
                    .AddField("Content:", message.Content)
                    .Build();
                await ReplyAsync(embed: embed);
            }
            else
            {
                await ReplyAsync("No messages with mentions found!");
            }
        }

        [Command("grab")]
        [Summary("grabs the last deleted message")]
        public async Task Grab()
        {
            var message = _guildService.delmsgs[Context.Guild.Id].LastOrDefault(x => x.MentionedUsers.Count == 0);
            if (!(message.Author is SocketGuildUser author)) return;

            if (message != null)
            {
                var role = author.Roles.OrderBy(x => x.Position).FirstOrDefault(x => x.Color != Color.Default);
                var color = role?.Color ?? Color.DarkPurple;

                var embed = new EmbedBuilder()
                    .WithCurrentTimestamp()
                    .WithColor(color)
                    .WithAuthor(author)
                    .WithTitle("Grabbed the last deleted message!")
                    .AddField("Content:", message.Content)
                    .Build();
                await ReplyAsync(embed: embed);
            }
            else
            {
                await ReplyAsync("No messages found!");
            }
        }
    }
}