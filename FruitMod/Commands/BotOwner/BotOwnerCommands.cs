using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Attributes;
using FruitMod.Database;
using FruitMod.Objects;
using FruitMod.Services;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace FruitMod.Commands.BotOwnerCommands
{
    [RequireOwner]
    [SetService]
    public class BotOwnerCommands : ModuleBase<FruitModContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly HttpClient _http;
        private readonly LaunchService _init;
        private readonly IServiceProvider _services;

        public BotOwnerCommands(DiscordSocketClient client, IServiceProvider services, HttpClient http, DbService db,
            LaunchService init)
        {
            _client = client;
            _services = services;
            _http = http;
            _db = db;
            _init = init;
        }

        [Command("ginfo")]
        [Summary("displays the bot's user count")]
        public async Task Users()
        {
            var guilds = _client.Guilds;
            var datastring = string.Join("\n\n",
                Context.Client.Guilds.Select(x =>
                    $"[Name: {x.Name}, ID: {x.Id}, Members: {x.MemberCount}, Owner: {x.Owner}]"));
            await ReplyAsync(
                $"Total Users: {guilds.Sum(x => x.MemberCount)} || Total Guilds: {guilds.Count}\n{Format.Code(datastring, "ini")}");
        }

        [Command("leave")]
        [Summary("forces the bot to leave the server")]
        public async Task Leave()
        {
            await Context.Guild.LeaveAsync();
        }

        [Command("leave")]
        [Summary("forces the bot to leave the server")]
        public async Task RLeave(ulong id)
        {
            await _client.GetGuild(id).LeaveAsync();
        }

        [Command("eval", RunMode = RunMode.Async)]
        [Summary("evaluates code")]
        public async Task Eval([Remainder] string code)
        {
            try
            {
                object result;
                var embed = new EmbedBuilder();
                var sopts = ScriptOptions.Default;
                var oldWriter = Console.Out;
                IEnumerable<string> systems = new[]
                {
                    "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks",
                    "System.Diagnostics", "Discord", "Discord.Commands", "Discord.WebSocket"
                };
                var imports = _init.Namespaces.Concat(systems);
                sopts = sopts.WithImports(imports);
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)));
                var globals = new EvalGlobals
                {
                    Context = Context,
                    Message = Context.SMessage,
                    Services = _services,
                    HttpClient = _http,
                    db = _db
                };
                var msg = await ReplyAsync("Evaluating....");
                File.Delete("out.txt");
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    using (var writer = new StreamWriter("out.txt"))
                    {
                        Console.SetOut(writer);
                        result = await CSharpScript.EvaluateAsync($"{code}", sopts, globals, typeof(EvalGlobals))
                            .ConfigureAwait(false);
                    }

                    Console.SetOut(oldWriter);
                    sw.Stop();
                    if (result != null)
                    {
                        embed.WithTitle($"Eval result: | Elapsed time: {sw.Elapsed.Humanize()}");
                        embed.AddField("Input: ", $"```{code}```");
                        embed.AddField("Result: ", $"```{result}```");
                        embed.WithThumbnailUrl("https://pluralsight.imgix.net/paths/path-icons/csharp-e7b8fcd4ce.png");
                        embed.Color = Color.Green;
                        await msg.DeleteAsync();
                        await ReplyAsync(string.Empty, false, embed.Build());
                    }
                    else
                    {
                        var nrmsg = File.ReadAllText("out.txt");
                        if (!code.Contains("Console")) nrmsg = "Success! No results returned!";
                        embed.WithTitle($"Eval result: | Elapsed time: {sw.Elapsed.Humanize()}");
                        embed.AddField("Input: ", $"```{code}```");
                        embed.AddField("Result: ", $"```{nrmsg}```");
                        embed.WithThumbnailUrl("https://pluralsight.imgix.net/paths/path-icons/csharp-e7b8fcd4ce.png");
                        embed.Color = Color.Green;
                        await msg.DeleteAsync();
                        await ReplyAsync(string.Empty, false, embed.Build());
                        File.Delete("out.txt");
                    }
                }
                catch (Exception e)
                {
                    Console.SetOut(oldWriter);
                    await msg.ModifyAsync(x => x.Content = $"Error! {e.Message}");
                }
                finally
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [Command("bblock", RunMode = RunMode.Async)]
        [Summary("Blocks or unblocks a user from using the bot!")]
        public async Task BBlock(IUser user)
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

        [Command("sudo")]
        [Alias("echo")]
        [Summary("sudo")]
        public async Task Echo([Remainder] string input)
        {
            if (Context.Message.Content.Contains("echo"))
            {
                await ReplyAsync(input);
            }
            else
            {
                if (!_db.GetById<GuildObjects>(Context.Guild.Id).Settings.Prefixes.Any(x => input.Contains(x)))
                    input = $"{_db.GetById<GuildObjects>(Context.Guild.Id).Settings.Prefixes[0]}{input}";
                var message = await ReplyAsync(input);
                await message.DeleteAsync();
                await ReplyAsync($"Sudo => {input}");
            }
        }

        [Command("relay")]
        [Summary("relays a chat")]
        public async Task Relay(ulong guildId)
        {
            var guild = _client.GetGuild(guildId);
            var channels = guild.TextChannels.OrderBy(x => x.Name).Select(y => y.Name);
            Console.WriteLine($"Please choose a channel:\n{string.Join("\n", channels)}");
            var response = await Console.In.ReadLineAsync();
            string channelname;

            if (channels.Any(x => x.Contains(response)))
            {
                channelname = channels.First(x => x.Contains(response));
            }
            else
            {
                Console.WriteLine("Channel does not exist. Case sensitive.");
                return;
            }

            var channel = guild.GetTextChannel(guild.TextChannels.FirstOrDefault(x => x.Name.Contains(channelname)).Id);
            await channel.SendMessageAsync(
                "Hello! The bot owner has connected to relay chat! I may now read and speak!");
            Context.Client.MessageReceived += RelayHandler;

            while (true)
            {
                Console.WriteLine("Ready to send a message!");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("[Your Message]: ");
                Console.ResetColor();
                response = await Console.In.ReadLineAsync();

                if (response == "exit")
                {
                    await channel.SendMessageAsync("The bot owner has disconnected from relay chat!");
                    Context.Client.MessageReceived -= RelayHandler;
                    return;
                }

                if (response == "channels")
                {
                    Console.WriteLine($"Please choose a channel:\n{string.Join("\n", channels)}");
                    response = await Console.In.ReadLineAsync();

                    if (channels.Contains(response))
                    {
                        channelname = channels.First(x => x.Equals(response));
                    }
                    else
                    {
                        Console.WriteLine("Channel does not exist. Case sensitive.");
                        return;
                    }

                    channel = guild.GetTextChannel(guild.TextChannels.FirstOrDefault(x => x.Name == channelname).Id);
                    response = string.Empty;
                }

                if (response != string.Empty)
                {
                    var mymsg = await channel.SendMessageAsync(response);
                }
            }

            async Task RelayHandler(SocketMessage msg)
            {
                if (!(msg is SocketUserMessage smsg)) return;
                if (smsg.Channel.Id != channel.Id) return;
                if (smsg.Author.IsBot) return;
                Console.ForegroundColor = ConsoleColor.Magenta;
                await Console.Out.WriteAsync("[Received Relay Message] ");
                Console.ResetColor();
                await Console.Out.WriteLineAsync($"{smsg.Author} wrote {smsg.Content}");
            }
        }

        [Command("dm")]
        [Summary("starts a dm with somebody")]
        public async Task Dm(ulong id)
        {
            var user = Context.Client.GetUser(id);
            try
            {
                await user.SendMessageAsync("The bot owner has started a chat with you!");
                var channel = await user.GetOrCreateDMChannelAsync();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("[DM channel connected!]");
                Console.ResetColor();
                Context.Client.MessageReceived += DMHandler;

                while (true)
                {
                    Console.WriteLine("Ready to send a message!");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[Your Message]: ");
                    Console.ResetColor();
                   var response = await Console.In.ReadLineAsync();

                    if (response == "exit")
                    {
                        await channel.SendMessageAsync("The bot owner has disconnected from relay chat!");
                        Context.Client.MessageReceived -= DMHandler;
                        return;
                    }

                    if (response != string.Empty)
                    {
                        var mymsg = await channel.SendMessageAsync(response);
                    }
                }

                async Task DMHandler(SocketMessage msg)
                {
                    if (msg.Channel.Id != channel.Id) return;
                    if (msg.Author.IsBot) return;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    await Console.Out.WriteAsync("[Received Relay Message] ");
                    Console.ResetColor();
                    await Console.Out.WriteLineAsync($"{msg.Author} wrote {msg.Content}");
                }

            }
            catch(Exception)
            {
            }
        }
    }
}