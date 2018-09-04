﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using FruitMod.Database;
using FruitMod.Interactive.Paginator;
using FruitMod.Objects;

namespace FruitMod.Commands
{
    public class Help : InteractiveBase
    {
        private CommandService CommandService { get; }
        private readonly DbService _db;

        public Help(CommandService commandService, DbService db)
        {
            CommandService = commandService;
            _db = db;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Displays all of the commands the bot may provide!")]
        public async Task HelpCommand()
        {
            var prefix = _db.GetById<GuildObjects>(Context.Guild.Id).Settings.Prefix;
            var pages = new List<string>();
            var modules = CommandService.Modules.Where(x => !x.Name.Contains("Owner"));
            foreach (var module in modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"<{cmd.Aliases.First()}> : `{cmd.Summary}`\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"**Module: __{module.Name}__**\n");
                    sb.AppendLine(description);
                    pages.Add(sb.ToString());

                }
            }

            var msg = new PaginatedMessage
            {
                Color = Color.Green,
                Options = new PaginatedAppearanceOptions { DisplayInformationIcon = false, JumpDisplayOptions = 0, Timeout = TimeSpan.FromSeconds(60)},
                Pages = pages,
                Author = new EmbedAuthorBuilder() { Name = Context.User.Username, IconUrl = Context.User.GetAvatarUrl() },
                Title = $"Commands you may use || Current Prefix: {prefix}"
            };
            await PagedReplyAsync(msg);
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Gives the summary of a command")]
        public async Task HelpCommand([Remainder] string command)
        {
            var cmd = CommandService.Commands.FirstOrDefault(x => x.Name == command);
            if (cmd?.Summary == null) await ReplyAsync($"Command {command} has no further explanation!");

            await ReplyAsync($"{command} => {cmd?.Summary}");
        }
    }
}