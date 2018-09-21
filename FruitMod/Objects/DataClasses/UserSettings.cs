using System.Collections.Generic;

namespace FruitMod.Objects.DataClasses
{
    public class UserSettings
    {
        public List<ulong> BlockedUsers = new List<ulong>();
        public List<ulong> BotBannedUsers = new List<ulong>();
        public List<ulong> MutedUsers = new List<ulong>();
        public Dictionary<ulong, List<(int, string)>> Warnings = new Dictionary<ulong, List<(int, string)>>();
    }
}