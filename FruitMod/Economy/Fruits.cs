using System.Collections.Generic;

namespace FruitMod.Economy
{
    public class Economy
    {
        public enum Fruit
        {
            Guavas,
            Grapes,
            Watermelons,
            Pineapples,
            Mangos
        }

        public static Dictionary<Fruit, int> fruitValues = new Dictionary<Fruit, int>
        {
            [Fruit.Guavas] = 1,
            [Fruit.Grapes] = 2,
            [Fruit.Watermelons] = 3,
            [Fruit.Pineapples] = 4,
            [Fruit.Mangos] = 5
        };
    }
}