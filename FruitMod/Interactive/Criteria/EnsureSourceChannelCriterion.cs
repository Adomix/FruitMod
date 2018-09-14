using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace FruitMod.Interactive.Criteria
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.Channel.Id == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}