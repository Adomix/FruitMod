using Discord;

namespace FruitMod.Extensions
{
    public static class CheckAttachments
    {
        public static bool HasAttachments(this IMessage msg)
            => msg.Attachments.Count > 0 ? true : false;
    }
}