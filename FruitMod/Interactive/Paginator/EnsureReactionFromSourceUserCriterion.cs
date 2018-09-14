using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Interactive.Criteria;

namespace FruitMod.Interactive.Paginator
{
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, SocketReaction parameter)
        {
            var ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}