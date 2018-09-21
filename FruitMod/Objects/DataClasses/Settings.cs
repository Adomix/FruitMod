using System.Collections.Generic;

namespace FruitMod.Objects.DataClasses
{
    public class Settings
    {
        public bool VoteSys { get; set; } = false;
        public bool LeaveSys { get; set; } = false;
        public bool DeleteSys { get; set; } = false;
        public List<string> Prefixes { get; set; } = new List<string>();
        public ulong? LogChannel { get; set; } = null;
        public ulong? InfoChannel { get; set; } = null;
        public ulong? MuteRole { get; set; } = null;
        public List<ulong> ModRoles { get; set; } = new List<ulong>();
        public List<ulong> AutoRoles { get; set; } = new List<ulong>();
    }
}