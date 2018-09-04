using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FruitMod.Objects
{
    public class GuildObjects
    {
        public List<ulong> BlockedUsers = new List<ulong>();
        public bool VoteSys { get; set; } = false;
        public bool LeaveSys { get; set; } = false;
        public bool DeleteSys { get; set; } = false;
        public string Prefix { get; set; }
        public ulong? LogChannel { get; set; } = null;
        public ulong? InfoChannel { get; set; } = null;
        public ulong? MuteRole { get; set; } = null;
        public ConcurrentDictionary<ulong, int> UserCurrency { get; set; } = new ConcurrentDictionary<ulong, int>();

        public List<ulong> MutedUsers { get; set; } = new List<ulong>();

        public ConcurrentQueue<string> MusicQueue { get; set; } = new ConcurrentQueue<string>();
    }
}