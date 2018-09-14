using System.Collections.Concurrent;
using FruitMod.Objects.DataClasses;

namespace FruitMod.Objects
{
    public class GuildObjects
    {
        public Settings Settings = new Settings();
        public UserSettings UserSettings = new UserSettings();

        public ConcurrentDictionary<ulong, int> UserCurrency { get; set; } = new ConcurrentDictionary<ulong, int>();

        public ConcurrentQueue<string> MusicQueue { get; set; } = new ConcurrentQueue<string>();
    }
}