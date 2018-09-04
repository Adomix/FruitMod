using System;
using System.Collections.Generic;
using System.Text;

namespace FruitMod.Objects.DataClasses
{
    public class Settings
    {
        public bool VoteSys { get; set; } = false;
        public bool LeaveSys { get; set; } = false;
        public bool DeleteSys { get; set; } = false;
        public string Prefix { get; set; } = "<@467236886616866816> ";
        public ulong? LogChannel { get; set; } = null;
        public List<ulong> MutedUsers { get; set; } = new List<ulong>();
        public ulong? InfoChannel { get; set; } = null;
        public ulong? MuteRole { get; set; } = null;
    }
}
