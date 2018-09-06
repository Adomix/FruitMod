using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Objects;
using Humanizer;

namespace FruitMod.Commands
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmd;
        private readonly DbService _db;

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
                //await Task.Delay(2000);
                var msg = await ReplyAsync("How to vote: Click the check for yes, the X for no!");
                await msg.AddReactionAsync(new Emoji("✅"));
                //await Task.Delay(2000);
                await msg.AddReactionAsync(new Emoji("❌"));
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

                if (check < xemj)
                {
                    await message.DeleteAsync();
                    await ReplyAsync($"{user} tonight, you are not being voted off the island.");
                }

                if (check == xemj)
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
            var uroles = Context.Guild.GetUser(user.Id).Roles.OrderBy(x => x.Position);
            var roles = string.Join(", ", uroles);
            var fixedroles = roles.Replace("@", string.Empty);
            var nickname = Context.Guild.GetUser(user.Id).Nickname;
            var game = user.Activity?.Name;

            if (Context.Guild.GetUser(user.Id).Nickname == null) nickname = "No nickname";

            if (user.Activity == null) game = "No game";

            var infoembed = new EmbedBuilder()
                .WithColor((user as SocketGuildUser).Roles.Last().Color)
                .WithTitle($"User: {user.Username}")
                .WithThumbnailUrl(user.GetAvatarUrl())
                .AddField("Nickname: ", nickname, true)
                .AddField("ID: ", user.Id, true)
                .AddField("Created: ", user.CreatedAt, true)
                .AddField("Joined: ", Context.Guild.GetUser(user.Id).JoinedAt, true)
                .AddField("Roles ", fixedroles, true)
                .AddField("Playing: ", game, true)
                .WithFooter($"Processed on: {DateTime.UtcNow:MM/dd/yyy hh:mm:ss}")
                .Build();
            await Context.Channel.SendMessageAsync(string.Empty, false, infoembed);
        }

        [Command("blocklist", RunMode = RunMode.Async)]
        [Summary("Displays current guild block list")]
        public async Task ViewBList()
        {
            var blocklist = _db.GetById<GuildObjects>(Context.Guild.Id).UserSettings.BlockedUsers;
            if (blocklist == null) return;
            var blockedembed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Block List")
                .AddField("Blocked IDs: ", string.Join(", ", blocklist))
                .WithFooter($"Processed on: {DateTime.UtcNow}")
                .Build();
            await Context.Channel.SendMessageAsync(string.Empty, false, blockedembed);
        }
    }
}