using System.Threading.Tasks;
using Discord.Commands;

namespace FruitMod.Interactive.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(ICommandContext sourceContext, T parameter)
        {
            return Task.FromResult(true);
        }
    }
}