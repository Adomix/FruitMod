using System;
using System.Collections.Generic;
using System.Text;

namespace FruitMod.Objects.DataClasses
{
    public class UserSettings
    {
        public List<ulong> MutedUsers { get; set; } = new List<ulong>();
        public List<ulong> BlockedUsers = new List<ulong>();
    }
}
