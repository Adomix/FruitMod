using System.Collections.Generic;

namespace FruitMod.Objects.DataClasses
{
    public class UserSettings
    {
        public List<ulong> BlockedUsers = new List<ulong>();
        public List<ulong> MutedUsers { get; set; } = new List<ulong>();
    }
}