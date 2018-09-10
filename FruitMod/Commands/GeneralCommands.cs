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
using Humanizer;

namespace FruitMod.Commands
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmd;
        private readonly DbService _db;

        private static Dictionary<ulong, DateTime> feedback = new Dictionary<ulong, DateTime>();

        public General(DiscordSocketClient client, CommandService cmd, DbService db)
        {
            _client = client;
            _cmd = cmd;
            _db = db;
        }

        [Command("info")]
        [Summary("Displays the bot's information")]
        public async Task Info()
        {
            var botInfo = await Context.Client.GetApplicationInfoAsync();
            var time = DateTime.Now - Process.GetCurrentProcess().StartTime;
            await ReplyAsync($"About me: {Format.Code($"Name: [{botInfo.Name}] Id: [{botInfo.Id}]\nOwner: [{botInfo.Owner}] Status: [{Context.Client.Status}]\nUptime: [{time.Humanize()}] Connection: [{Context.Client.ConnectionState}]\nModules: [{_cmd.Modules.Count()}] Commands: [{_cmd.Commands.Count()}]\nSource: [https://github.com/Adomix/FruitMod]", "ini")}");
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
            var info = _cmd.Modules.OrderBy(x => x.Name);
            var mods = info.Select(x => x.Name);
            var modules = string.Join(", ", mods);
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
            await msg.ModifyAsync(x => x.Content = $":clap: Ping: {sw.ElapsedMilliseconds}ms || :handshake: API: {_client.Latency}ms");
        }

        [Command("uptime")]
        [Summary("Gives the uptime of the bot")]
        public async Task Uptime()
        {
            var time = DateTime.Now - Process.GetCurrentProcess().StartTime;
            var human = time.Humanize();
            await ReplyAsync($"The bot has been online for: {human}");
        }

        [Command("votekick", RunMode = RunMode.Async)]
        [Summary("Vote kicks a member, usage: votekick <user> <reason(optional)>")]
        public async Task VoteKick(IUser user, [Remainder] string reason = null)
        {
            if (_db.GetById<GuildObjects>(Context.Guild.Id) == null)
            {
                await ReplyAsync("Creating database! Please try again!");
                return;
            }

            if (_db.GetById<GuildObjects>(Context.Guild.Id).Settings.VoteSys == false)
            {
                await ReplyAsync("An administrator needs to turn this feature on!");
            }
            else
            {
                await ReplyAsync(
                    $"RIP! {Context.User.Username} has initiated a votekick against {user.Username} for {reason}! You have 60 seconds to vote! Vote opens in 2 seconds!");
                var msg = await ReplyAsync("How to vote: Click the check for yes, the X for no!");
                await msg.AddReactionsAsync(new Emoji[] { new Emoji("✅"), new Emoji("❌") });
                await Task.Delay(60000);
                var mess = await Context.Channel.GetMessageAsync(msg.Id);
                var message = mess as IUserMessage;
                var check = message?.Reactions
                    .GetValueOrDefault(message.Reactions.Keys.FirstOrDefault(x => x.Name == "✅")).ReactionCount;
                var xemj = message?.Reactions
                    .GetValueOrDefault(message.Reactions.Keys.FirstOrDefault(x => x.Name == "❌")).ReactionCount;
                if (check > xemj)
                {
                    await message.DeleteAsync();
                    await ReplyAsync($"{user}, the tribe has spoken.");
                    await user.SendMessageAsync(
                        $"Kick details: Voted off by {Context.User.Username} for the reason {reason}");
                    await Context.Guild.GetUser(user.Id).KickAsync();
                }
                else if (check < xemj)
                {
                    await message.DeleteAsync();
                    await ReplyAsync($"{user} tonight, you are not being voted off the island.");
                }
                else if (check == xemj)
                {
                    if (message != null) await message.DeleteAsync();
                    await ReplyAsync($"{user}, the tribe is inconclusive, enjoy your stay.");
                }
            }
        }

        [Command("userinfo", RunMode = RunMode.Async)]
        [Summary("Gathers info on a user, usage: userinfo <user>")]
        public async Task UserInfo([Remainder] IUser user)
        {

            if (!(user is SocketGuildUser suser)) return;

            var perms = string.Join(", ", suser.GuildPermissions.ToList());
            if (suser.GuildPermissions.ToList().Contains(GuildPermission.Administrator)) perms = "All permissions!";
            if (suser.GuildPermissions.ToList().Count > 5) perms = $"`{string.Join(", ", suser.GuildPermissions.ToList())}`";

            var roles = string.Join(", ", suser.Roles.OrderBy(x => x.Name));
            if (suser.Roles.ToList().Select(x => x.Name).Count() > 5) roles = $"`{string.Join(", ", suser.Roles.OrderBy(x => x.Name))}`";

            Color color;

            if (!(suser.Roles.Contains(suser.Roles.LastOrDefault(x => x.Color != Color.Default))))
            {
                color = Color.DarkPurple;
            }
            else
            {
                color = suser.Roles.LastOrDefault(x => x.Color != Color.Default).Color;
            }
            var infoembed = new EmbedBuilder()

                .WithColor(color)
                .WithTitle($"User: {suser.Username}")
                .WithThumbnailUrl(suser.GetAvatarUrl())
                .WithCurrentTimestamp()

                .AddField("Nickname:", suser.Nickname ?? "No nickname", true)
                .AddField("ID:", suser.Id, true)

                .AddField("Discriminator:", suser.Discriminator, true)
                .AddField("Bot:", suser.IsBot, true)

                .AddField("Created:", suser.CreatedAt.Date, true)
                .AddField("Joined:", suser.JoinedAt.Value.Date, true)

                .AddField("Highest Role:", suser.Roles.Last(), true)
                .AddField("User Hierarchy:", $"{((suser.Hierarchy == int.MaxValue) ? "Guild Owner" : $"{ suser.Hierarchy}")}", true)

                .AddField("All Roles:", roles)

                .AddField("Permissions:", perms)

                .AddField("Playing:", suser.Activity?.Name ?? "Not currently playing anything", true)

                .Build();
            await Context.Channel.SendMessageAsync(string.Empty, false, infoembed);
        }

        [Command("avatar")]
        [Summary("Shows the users avatar. Usage: avatar <user>(optional)")]
        public async Task Avatar([Remainder] IUser user = null)
        {

            if (user == null) user = Context.User;

            if (!(user is SocketGuildUser suser)) return;

            Color color;

            if (!(suser.Roles.Contains(suser.Roles.LastOrDefault(x => x.Color != Color.Default))))
            {
                color = Color.DarkPurple;
            }
            else
            {
                color = suser.Roles.LastOrDefault(x => x.Color != Color.Default).Color;
            }

            var embed = new EmbedBuilder()
                .WithColor(color)
                .WithImageUrl(suser.GetAvatarUrl(size: 1024) ?? "User has the default avatar!")
                .Build();
            await ReplyAsync(string.Empty, false, embed);
        }

        [Command("feedback")]
        [Summary("Gives the bot owner feedback")]
        public async Task Feedback([Remainder] string message)
        {
            if (feedback.ContainsKey(Context.User.Id) && feedback[Context.User.Id].Date == DateTime.Now.Date) { await ReplyAsync("You have already sent some feedback today!"); return; }
            feedback.Add(Context.User.Id, DateTime.Now);
            await _client.GetUser(386969677143736330).SendMessageAsync($"Feedback from {Context.User} : {message}");
            await ReplyAsync("Thank you! Your feedback has been sent to the bot owner!");
        }

        [Command("blocklist", RunMode = RunMode.Async)]
        [Summary("Displays current guild block list")]
        public async Task ViewBList()
        {
            var blocklist = _db.GetById<GuildObjects>(Context.Guild.Id).UserSettings.BlockedUsers;
            if (blocklist == null)
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
            await Context.Channel.SendMessageAsync(string.Empty, false, blockedembed);
        }
    }
}