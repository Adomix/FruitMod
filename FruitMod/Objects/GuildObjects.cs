using FruitMod.Objects.DataClasses;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FruitMod.Objects
{
    public class GuildObjects
    {
        public UserSettings UserSettings = new UserSettings();

        public Settings Settings = new Settings();

        public ConcurrentDictionary<ulong, int> UserCurrency { get; set; } = new ConcurrentDictionary<ulong, int>();

        public ConcurrentQueue<string> MusicQueue { get; set; } = new ConcurrentQueue<string>();
    }
}