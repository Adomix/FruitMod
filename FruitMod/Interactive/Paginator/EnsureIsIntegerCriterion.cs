using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using FruitMod.Interactive.Criteria;

namespace FruitMod.Interactive.Paginator
{
    internal class EnsureIsIntegerCriterion : ICriterion<SocketMessage>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, SocketMessage parameter)
        {
            var ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}