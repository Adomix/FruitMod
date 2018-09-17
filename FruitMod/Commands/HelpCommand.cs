using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Database;
using FruitMod.Interactive.Paginator;
using FruitMod.Objects;
using Octokit;
using Octokit.Internal;

namespace FruitMod.Commands
{
    public class Help : InteractiveBase
    {
        private static readonly GitHubClient github = new GitHubClient(new ProductHeaderValue("Adomix"),
            new InMemoryCredentialStore(new Credentials(ConfigurationManager.AppSettings["githubtoken"])));

        private readonly DbService _db;
        private readonly IServiceProvider _provider;

        public Help(CommandService commandService, DbService db, IServiceProvider provider)
        {
            CommandService = commandService;
            _db = db;
            _provider = provider;
        }

        private CommandService CommandService { get; }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Displays all of the commands the bot may provide!")]
        public async Task HelpCommand()
        {
            var prefixes = string.Join(", ", _db.GetById<GuildObjects>(Context.Guild.Id).Settings.Prefixes);
            var pages = new List<string>();
            var modules = CommandService.Modules.Where(x => !x.Name.Contains("Owner"));

            Color color;
            if (!Context.GuildUser.Roles.Contains(Context.GuildUser.Roles.LastOrDefault(x => x.Color != Color.Default)))
                color = Color.DarkPurple;
            else
                color = Context.GuildUser.Roles.LastOrDefault(x => x.Color != Color.Default).Color;

            foreach (var module in modules)
            {
                string description = null;
                foreach (var cmd in module.Commands.Distinct())
                {
                    var result = await cmd.CheckPreconditionsAsync(Context, _provider);
                    if (result.IsSuccess)
                        description += $"**__{cmd.Aliases.First()}__** : => {cmd.Summary ?? "no summary provided"}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"**Module: __{module.Name}__**\n");
                    sb.AppendLine(description);
                    pages.Add(sb.ToString());
                }
            }

            var msg = new PaginatedMessage
            {
                Color = color,
                Options = new PaginatedAppearanceOptions
                    {DisplayInformationIcon = false, JumpDisplayOptions = 0, Timeout = TimeSpan.FromSeconds(60)},
                Pages = pages,
                Author = new EmbedAuthorBuilder {Name = Context.User.Username, IconUrl = Context.User.GetAvatarUrl()},
                Title = $"Commands you may use || Current Prefix(es): {prefixes}"
            };

            await PagedReplyAsync(msg);
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Gives the summary of a command. Usage help <command>")]
        public async Task HelpCommand([Remainder] string command)
        {
            var cmd = CommandService.Commands.FirstOrDefault(x => x.Name == command);
            if (cmd?.Summary == null) await ReplyAsync($"Command {command} has no further explanation!");

            await ReplyAsync($"{command} => {cmd?.Summary}");
        }

        [Command("git")]
        [Summary("Shows the latest github push")]
        public async Task Git()
        {
            var channel = Context.Client.GetChannel(487463564592939030) as SocketTextChannel;
            var msg = (await channel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();
            var repo = await github.Repository.Get("Adomix", "FruitMod");
            var commits = await github.Repository.Commit.GetAll("Adomix", "FruitMod");

            var embed = new EmbedBuilder()
                .WithAuthor(Context.User)
                .WithThumbnailUrl(repo.Owner.AvatarUrl)
                .WithColor(Color.DarkPurple)
                .WithTitle(repo.Name)
                .AddField("Description:", repo.Description ?? "No description")
                .AddField("Commits:", string.Join("\n", commits.Select(x => x.Commit.Message).Take(3)) ?? "No commits")
                .Build();

            await ReplyAsync(string.Empty, false, embed);

            await ReplyAsync(string.Empty, false, msg.Embeds.FirstOrDefault() as Embed);
        }
    }
}