using System.Threading.Tasks;
using Discord;

namespace FruitMod.Interactive.Callbacks
{
    public interface ICallback
    {
        Task DisplayAsync();
        IUserMessage Message { get; }
    }
}
