using System.Threading.Tasks;
using Discord;

namespace FruitMod.Extensions
{
    public static class IUserExtension
    {
        public static Task<IUserMessage> TryDMAsync(this IUser user, string content = null, bool isTTS = false,
            Embed embed = null, RequestOptions options = null)
        {
            try
            {
                return user.SendMessageAsync(content, isTTS, embed, options);
            }
            catch
            {
                return null;
            }
        }
    }
}