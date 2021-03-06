﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Interactive.Callbacks;
using FruitMod.Interactive.Criteria;

namespace FruitMod.Interactive.Paginator
{
    public class PaginatedMessageCallback : IReactionCallback, ICallback
    {
        private readonly InteractiveService _interactive;
        private readonly PaginatedMessage _pager;
        private readonly int _pages;
        private int _page = 1;

        public PaginatedMessageCallback(InteractiveService interactive,
            ICommandContext sourceContext,
            PaginatedMessage pager, ICriterion<SocketReaction> criterion = null)
        {
            _interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _pager = pager;
            _pages = _pager.Pages.Count();
        }

        private PaginatedAppearanceOptions Options => _pager.Options;

        public IUserMessage Message { get; private set; }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(_pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            _interactive.AddReactionCallback(message, this);
            _ = Task.Run(async () =>
            {
                //BypassBuckets flag is a property that is part of my modified form of the lib
                await message.AddReactionAsync(Options.First);
                await message.AddReactionAsync(Options.Back);
                await message.AddReactionAsync(Options.Next);
                await message.AddReactionAsync(Options.Last);

                var manageMessages = Context.Channel is IGuildChannel guildChannel
                    ? (Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages
                    : false;

                if (Options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages)
                    await message.AddReactionAsync(Options.Jump);

                await message.AddReactionAsync(Options.Stop);

                if (Options.DisplayInformationIcon)
                    await message.AddReactionAsync(Options.Info);
            });
            if (Timeout.HasValue && Timeout.Value != null)
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    _interactive.RemoveReactionCallback(message);
                    _ = Message.DeleteAsync();
                });
        }

        public ICommandContext Context { get; }
        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion { get; }
        public TimeSpan? Timeout => TimeSpan.FromMinutes(2);

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(Options.First))
            {
                _page = 1;
            }
            else if (emote.Equals(Options.Next))
            {
                if (_page >= _pages)
                {
                    _page = 1;
                    return false;
                }

                ++_page;
            }
            else if (emote.Equals(Options.Back))
            {
                if (_page <= 1)
                {
                    _page = _pages;
                    return false;
                }

                --_page;
            }
            else if (emote.Equals(Options.Last))
            {
                _page = _pages;
            }
            else if (emote.Equals(Options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(Options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await _interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > _pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await _interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name);
                        return;
                    }

                    _page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(Options.Info))
            {
                await _interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
                return false;
            }

            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }

        protected Embed BuildEmbed()
        {
            return new EmbedBuilder()
                .WithAuthor(_pager.Author)
                .WithColor(_pager.Color)
                .WithDescription(_pager.Pages.ElementAt(_page - 1).ToString())
                .WithFooter(f => f.Text = string.Format(Options.FooterFormat, _page, _pages))
                .WithTitle(_pager.Title)
                .Build();
        }

        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}