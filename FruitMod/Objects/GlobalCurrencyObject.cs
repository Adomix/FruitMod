using System.Collections.Generic;
using static FruitMod.Economy.Store;

namespace FruitMod.Objects
{
    public class GlobalCurrencyObject
    {
        public Dictionary<ulong, int> GuildCurrencyValue = new Dictionary<ulong, int>();
        public Dictionary<ulong, int> AutomatedGuilds = new Dictionary<ulong, int>();
        public Dictionary<ulong, List<Shop>> GuildModifiers = new Dictionary<ulong, List<Shop>>();
    }
}
