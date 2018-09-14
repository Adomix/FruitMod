using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Interactive.Criteria;

namespace FruitMod.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        ICommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}