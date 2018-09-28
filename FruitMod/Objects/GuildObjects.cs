using System.Collections.Concurrent;
using System.Collections.Generic;
using FruitMod.Objects.DataClasses;

namespace FruitMod.Objects
{
    public class GuildObjects
    {
        public Settings Settings = new Settings();
        public UserSettings UserSettings = new UserSettings();

        public ConcurrentQueue<string> MusicQueue { get; set; } = new ConcurrentQueue<string>();

        public Dictionary<ulong, UserStruct> UserStruct { get; set; } = new Dictionary<ulong, UserStruct>();
    }
}