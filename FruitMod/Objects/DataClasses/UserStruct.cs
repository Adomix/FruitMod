using System.Collections.Generic;
using static FruitMod.Economy.Economy;

namespace FruitMod.Objects.DataClasses
{
    public class UserStruct
    {
        public ulong UserId;
        public int Warnings;
        public Dictionary<Fruit, int> Fruit;
    }
}