using Discord;

namespace FruitMod.Extensions
{
    public static class CheckAttachments
    {
        public static bool HasAttachments(this IMessage msg)
        {
            return msg.Attachments.Count > 0;
        }
    }
}