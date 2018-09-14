using System.Threading.Tasks;
using Discord;

namespace FruitMod.Interactive.Callbacks
{
    public interface ICallback
    {
        IUserMessage Message { get; }
        Task DisplayAsync();
    }
}