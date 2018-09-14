using System.Threading.Tasks;
using Discord.Commands;

namespace FruitMod.Interactive.Criteria
{
    public interface ICriterion<in T>
    {
        Task<bool> JudgeAsync(ICommandContext sourceContext, T parameter);
    }
}