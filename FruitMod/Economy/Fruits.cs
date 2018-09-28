using System.Collections.Generic;

namespace FruitMod.Economy
{
    public class Economy
    {
        public enum Fruit
        {
            mangos,
            watermelons
        }

        public static Dictionary<Fruit, int> fruitValues = new Dictionary<Fruit, int>
        {
            [Fruit.mangos] = 1,
            [Fruit.watermelons] = 2
        };
    }
}