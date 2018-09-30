using System.Collections.Generic;

namespace FruitMod.Objects.DataClasses
{
    public class Settings
    {
        public List<string> Prefixes { get; set; } = new List<string>();

        public bool VoteSys { get; set; } 
        public bool LeaveSys { get; set; }
        public bool DeleteSys { get; set; } 

        public List<ulong> ModRoles { get; set; } = new List<ulong>();
        public List<ulong> AutoRoles { get; set; } = new List<ulong>();

        public ulong? LogChannel { get; set; }
        public ulong? InfoChannel { get; set; }
        public ulong? MuteRole { get; set; }
    }
}
