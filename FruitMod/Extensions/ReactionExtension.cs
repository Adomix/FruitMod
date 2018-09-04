using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace FruitMod.Extensions
{
    public static class ReactionExtension
    {
        public static async Task AddReactionsAsync(this IUserMessage msg, IEnumerable<IEmote> emotes)
        {
            foreach (var emote in emotes) await msg.AddReactionAsync(emote);
        }
    }
}