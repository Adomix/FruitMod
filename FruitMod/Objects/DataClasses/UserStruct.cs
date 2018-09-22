using FruitMod.Economy;
using System.Collections.Generic;

namespace FruitMod.Objects.DataClasses
{
    public struct UserStruct
    {
        public ulong UserId;
        public int Warnings;
        public Dictionary<Fruits, int> Fruits;
    }
}