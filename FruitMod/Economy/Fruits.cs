using System.Collections.Generic;

namespace FruitMod.Economy
{
    public class Economy
    {
        public enum Fruit
        {
            watermelons,
            pineapples,
            mangos
        }

        public static Dictionary<Fruit, int> fruitValues = new Dictionary<Fruit, int>
        {
            [Fruit.watermelons] = 1,
            [Fruit.pineapples] = 2,
            [Fruit.mangos] = 3
        };
    }
}